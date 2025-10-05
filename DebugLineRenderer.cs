using System.Collections.Generic;
using UnityEngine;

namespace silksong_Aiming {

    public class DebugLineRenderer : MonoBehaviour {
        private static DebugLineRenderer instance;

        // 线条池设置
        private const int initialPoolSize = 20;
        private Queue<LineRenderer> linePool = new Queue<LineRenderer>();
        private List<LineRenderer> activeLines = new List<LineRenderer>();

        // 默认线条设置
        public float defaultWidth = 0.05f;
        public Material defaultMaterial;

        void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else {
                Destroy(gameObject);
            }
        }
        void InitializePool() {
            // 创建默认材质（如果未提供）
            if (defaultMaterial == null) {
                defaultMaterial = new Material(Shader.Find("Sprites/Default"));
                defaultMaterial.color = Color.white;
                defaultMaterial.renderQueue = 4050;
            }

            // 初始化对象池
            for (int i = 0; i < initialPoolSize; i++) {
                CreateLineInPool();
            }
        }

        void CreateLineInPool() {
            GameObject lineObj = new GameObject("DebugLine");
            lineObj.transform.SetParent(transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = defaultMaterial;
            lr.startWidth = defaultWidth;
            lr.endWidth = defaultWidth;
            lr.positionCount = 0;
            lr.useWorldSpace = true;
            lr.enabled = false;

            linePool.Enqueue(lr);
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f, float width = -1f) {
            if (instance == null) CreateInstance();
            instance.DrawLineInternal(start, end, color, duration, width);
        }

        public static void DrawRay(Vector3 origin, Vector3 direction, Color color, float duration = 0f, float width = -1f) {
            DrawLine(origin, origin + direction, color, duration, width);
        }

        private void DrawLineInternal(Vector3 start, Vector3 end, Color color, float duration, float width) {
            LineRenderer lr = GetAvailableLine();
            if (lr == null) return;

            // 设置线条属性
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 设置颜色
            lr.startColor = color;
            lr.endColor = color;

            // 设置宽度
            float actualWidth = width > 0 ? width : defaultWidth;
            lr.startWidth = actualWidth;
            lr.endWidth = actualWidth;

            // 激活线条
            lr.enabled = true;
            activeLines.Add(lr);

            // 设置自动销毁（如果指定了持续时间）
            if (duration > 0) {
                StartCoroutine(ReturnLineAfterDelay(lr, duration));
            }
        }

        private LineRenderer GetAvailableLine() {
            // 尝试从池中获取
            if (linePool.Count > 0) {
                return linePool.Dequeue();
            }

            // 池为空，创建新线条
            CreateLineInPool();
            return linePool.Dequeue();
        }

        private System.Collections.IEnumerator ReturnLineAfterDelay(LineRenderer lr, float delay) {
            yield return new WaitForSeconds(delay);
            ReturnLineToPool(lr);
        }

        public static void ClearAllLines() {
            if (instance == null) return;

            foreach (LineRenderer lr in instance.activeLines.ToArray()) {
                instance.ReturnLineToPool(lr);
            }
        }

        private void ReturnLineToPool(LineRenderer lr) {
            if (lr == null) return;

            // 重置线条
            lr.positionCount = 0;
            lr.enabled = false;

            // 从活动列表移除
            activeLines.Remove(lr);

            // 返回池中
            linePool.Enqueue(lr);
        }

        private static void CreateInstance() {
            GameObject go = new GameObject("DebugLineRenderer");
            instance = go.AddComponent<DebugLineRenderer>();
        }

        // 高级功能：绘制带箭头的线
        public static void DrawArrow(Vector3 start, Vector3 end, Color color, float arrowSize = 0.2f, float duration = 0f, float width = -1f) {
            DrawLine(start, end, color, duration, width);

            // 计算箭头方向
            Vector3 direction = (end - start).normalized;

            // 绘制箭头两边
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 arrowPoint1 = end - direction * arrowSize + perpendicular * arrowSize * 0.5f;
            Vector3 arrowPoint2 = end - direction * arrowSize - perpendicular * arrowSize * 0.5f;

            DrawLine(end, arrowPoint1, color, duration, width);
            DrawLine(end, arrowPoint2, color, duration, width);
        }

        // 绘制贝塞尔曲线
        public static void DrawBezierCurve(Vector3 start, Vector3 control, Vector3 end, Color color, int segments = 20, float duration = 0f, float width = -1f) {
            if (instance == null) CreateInstance();

            LineRenderer lr = instance.GetAvailableLine();
            if (lr == null) return;

            // 设置曲线点
            lr.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++) {
                float t = i / (float)segments;
                Vector3 position = CalculateBezierPoint(t, start, control, end);
                lr.SetPosition(i, position);
            }

            // 设置颜色和宽度
            lr.startColor = color;
            lr.endColor = color;

            float actualWidth = width > 0 ? width : instance.defaultWidth;
            lr.startWidth = actualWidth;
            lr.endWidth = actualWidth;

            // 激活线条
            lr.enabled = true;
            instance.activeLines.Add(lr);

            // 设置自动销毁
            if (duration > 0) {
                instance.StartCoroutine(instance.ReturnLineAfterDelay(lr, duration));
            }
        }

        private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = uu * p0;
            p += 2 * u * t * p1;
            p += tt * p2;

            return p;
        }

        // 绘制网格
        public static void DrawGrid(Vector3 center, Vector2 size, int divisions, Color color, float duration = 0f, float width = -1f) {
            float cellSizeX = size.x / divisions;
            float cellSizeY = size.y / divisions;

            Vector3 startX = center - new Vector3(size.x / 2, 0, 0);
            Vector3 startY = center - new Vector3(0, size.y / 2, 0);

            // 绘制水平线
            for (int i = 0; i <= divisions; i++) {
                Vector3 lineStart = startX + new Vector3(0, i * cellSizeY, 0);
                Vector3 lineEnd = lineStart + new Vector3(size.x, 0, 0);
                DrawLine(lineStart, lineEnd, color, duration, width);
            }

            // 绘制垂直线
            for (int i = 0; i <= divisions; i++) {
                Vector3 lineStart = startY + new Vector3(i * cellSizeX, 0, 0);
                Vector3 lineEnd = lineStart + new Vector3(0, size.y, 0);
                DrawLine(lineStart, lineEnd, color, duration, width);
            }
        }

        // 绘制圆形
        public static void DrawCircle(Vector3 center, float radius, Color color, int segments = 32, float duration = 0f, float width = -1f) {
            if (instance == null) CreateInstance();

            LineRenderer lr = instance.GetAvailableLine();
            if (lr == null) return;

            // 设置圆形点
            lr.positionCount = segments + 1;
            lr.loop = true;

            for (int i = 0; i <= segments; i++) {
                float angle = i / (float)segments * Mathf.PI * 2;
                Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                lr.SetPosition(i, position);
            }

            // 设置颜色和宽度
            lr.startColor = color;
            lr.endColor = color;

            float actualWidth = width > 0 ? width : instance.defaultWidth;
            lr.startWidth = actualWidth;
            lr.endWidth = actualWidth;

            // 激活线条
            lr.enabled = true;
            instance.activeLines.Add(lr);

            // 设置自动销毁
            if (duration > 0) {
                instance.StartCoroutine(instance.ReturnLineAfterDelay(lr, duration));
            }
        }

        // 绘制矩形
        public static void DrawRectangle(Vector3 center, Vector2 size, Color color, float duration = 0f, float width = -1f) {
            Vector3 topLeft = center + new Vector3(-size.x / 2, size.y / 2, 0);
            Vector3 topRight = center + new Vector3(size.x / 2, size.y / 2, 0);
            Vector3 bottomRight = center + new Vector3(size.x / 2, -size.y / 2, 0);
            Vector3 bottomLeft = center + new Vector3(-size.x / 2, -size.y / 2, 0);

            DrawLine(topLeft, topRight, color, duration, width);
            DrawLine(topRight, bottomRight, color, duration, width);
            DrawLine(bottomRight, bottomLeft, color, duration, width);
            DrawLine(bottomLeft, topLeft, color, duration, width);
        }

        // 绘制3D立方体
        public static void DrawCube(Vector3 center, Vector3 size, Color color, float duration = 0f, float width = -1f) {
            Vector3 halfSize = size / 2;

            // 底部
            Vector3 bfl = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            Vector3 bfr = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            Vector3 bbr = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Vector3 bbl = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);

            // 顶部
            Vector3 tfl = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            Vector3 tfr = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            Vector3 tbr = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            Vector3 tbl = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // 底部矩形
            DrawLine(bfl, bfr, color, duration, width);
            DrawLine(bfr, bbr, color, duration, width);
            DrawLine(bbr, bbl, color, duration, width);
            DrawLine(bbl, bfl, color, duration, width);

            // 顶部矩形
            DrawLine(tfl, tfr, color, duration, width);
            DrawLine(tfr, tbr, color, duration, width);
            DrawLine(tbr, tbl, color, duration, width);
            DrawLine(tbl, tfl, color, duration, width);

            // 垂直边
            DrawLine(bfl, tfl, color, duration, width);
            DrawLine(bfr, tfr, color, duration, width);
            DrawLine(bbr, tbr, color, duration, width);
            DrawLine(bbl, tbl, color, duration, width);
        }

        // 绘制路径
        public static void DrawPath(Vector3[] points, Color color, bool loop = false, float duration = 0f, float width = -1f) {
            if (points == null || points.Length < 2) return;
            if (instance == null) CreateInstance();

            LineRenderer lr = instance.GetAvailableLine();
            if (lr == null) return;

            // 设置路径点
            lr.positionCount = points.Length;
            lr.SetPositions(points);
            lr.loop = loop;

            // 设置颜色和宽度
            lr.startColor = color;
            lr.endColor = color;

            float actualWidth = width > 0 ? width : instance.defaultWidth;
            lr.startWidth = actualWidth;
            lr.endWidth = actualWidth;

            // 激活线条
            lr.enabled = true;
            instance.activeLines.Add(lr);

            // 设置自动销毁
            if (duration > 0) {
                instance.StartCoroutine(instance.ReturnLineAfterDelay(lr, duration));
            }
        }

        // 绘制带渐变的线
        public static void DrawGradientLine(Vector3 start, Vector3 end, Color startColor, Color endColor, float duration = 0f, float width = -1f) {
            if (instance == null) CreateInstance();

            LineRenderer lr = instance.GetAvailableLine();
            if (lr == null) return;

            // 设置线条点
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            // 设置渐变颜色
            lr.startColor = startColor;
            lr.endColor = endColor;

            // 设置宽度
            float actualWidth = width > 0 ? width : instance.defaultWidth;
            lr.startWidth = actualWidth;
            lr.endWidth = actualWidth;

            // 激活线条
            lr.enabled = true;
            instance.activeLines.Add(lr);

            // 设置自动销毁
            if (duration > 0) {
                instance.StartCoroutine(instance.ReturnLineAfterDelay(lr, duration));
            }
        }

        // 绘制虚线
        public static void DrawDashedLine(Vector3 start, Vector3 end, Color color, float dashLength = 0.2f, float gapLength = 0.1f, float duration = 0f, float width = -1f) {
            Vector3 direction = (end - start).normalized;
            float totalLength = Vector3.Distance(start, end);
            float segmentLength = dashLength + gapLength;
            int segments = Mathf.FloorToInt(totalLength / segmentLength);

            for (int i = 0; i < segments; i++) {
                float startOffset = i * segmentLength;
                float endOffset = startOffset + dashLength;

                if (endOffset > totalLength) endOffset = totalLength;

                Vector3 dashStart = start + direction * startOffset;
                Vector3 dashEnd = start + direction * endOffset;

                DrawLine(dashStart, dashEnd, color, duration, width);
            }
        }

        // 绘制带文本标签的线
        public static void DrawLineWithLabel(Vector3 start, Vector3 end, Color color, string label, float duration = 0f, float width = -1f) {
            DrawLine(start, end, color, duration, width);

            // 计算标签位置（线段中点）
            Vector3 labelPos = (start + end) / 2;

            // 创建文本标签
            CreateTextLabel(labelPos, label, color, duration);
        }

        private static void CreateTextLabel(Vector3 position, string text, Color color, float duration) {
            if (instance == null) CreateInstance();

            GameObject labelObj = new GameObject("DebugLabel");
            labelObj.transform.SetParent(instance.transform);

            // 添加文本组件
            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 20;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // 设置位置
            labelObj.transform.position = position;

            // 设置自动销毁
            if (duration > 0) {
                instance.StartCoroutine(instance.DestroyAfterDelay(labelObj, duration));
            }
        }

        private System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay) {
            yield return new WaitForSeconds(delay);
            Destroy(obj);
        }
    }
}
