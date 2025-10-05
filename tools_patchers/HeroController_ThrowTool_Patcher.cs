using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using GlobalSettings;
using HutongGames.PlayMaker;
using System.Linq;

namespace silksong_Aiming {
    [HarmonyPatch(typeof(HeroController), "ThrowTool", new Type[] { typeof(bool) })]
    class HeroController_ThrowTool_Patcher {
        static bool Prefix(HeroController __instance, bool isAutoThrow) {
            if (!AimingManager.IsAiming) return true;
            try {
                Vector3 mouseWorldPos = AimingManager.RefreshMousePosition();

                // 获取角色位置
                Vector3 heroPos = __instance.transform.position;

                // 使用反射获取私有字段
                double canThrowTime = (double)(AccessTools.Field(typeof(HeroController), "canThrowTime").GetValue(__instance));
                var willThrowTool = AccessTools.Field(typeof(HeroController), "willThrowTool").GetValue(__instance) as ToolItem;
                var cState = __instance.cState;
                var toolThrowPoint = AccessTools.Field(typeof(HeroController), "toolThrowPoint").GetValue(__instance) as Transform;
                var toolThrowWallPoint = AccessTools.Field(typeof(HeroController), "toolThrowWallPoint").GetValue(__instance) as Transform;
                var toolThrowClosePoint = AccessTools.Field(typeof(HeroController), "toolThrowClosePoint").GetValue(__instance) as Transform;
                var inputHandler = AccessTools.Field(typeof(HeroController), "inputHandler").GetValue(__instance) as InputHandler;
                var animCtrl = AccessTools.Field(typeof(HeroController), "animCtrl").GetValue(__instance) as HeroAnimationController;
                var vibrationCtrl = AccessTools.Field(typeof(HeroController), "vibrationCtrl").GetValue(__instance) as HeroVibrationController;
                var toolEventTarget = AccessTools.Field(typeof(HeroController), "toolEventTarget").GetValue(__instance) as PlayMakerFSM;

                if (!isAutoThrow && Time.timeAsDouble < canThrowTime) {
                    return true;
                }
                if (willThrowTool == null) return true;

                if (willThrowTool.Type == ToolItemType.Skill) {
                    return true;
                }

                ToolItem.UsageOptions usage = willThrowTool.Usage;
                ToolItemsData.Data savedData = willThrowTool.SavedData;
                // 非技能工具处理
                // 如果没有投掷预制体
                if (!usage.ThrowPrefab) {
                    //Debug.Log("没有投掷预制体");
                    //Debug.Log(usage.FsmEventName);
                    string[] toPatcher = { "TRI PIN", "TACKS", "FISHERPIN", "WEBSHOT FORGE", "WEBSHOT ARCHITECT", "WEBSHOT WEAVER", "ROSARY CANNON" };
                    if (!toPatcher.Contains(usage.FsmEventName)) {
                        // 原方法处理
                        return true;
                    }
                }
                if (usage.ThrowPrefab) {
                    //Debug.Log(usage.ThrowPrefab.name);
                    string[] toPatcher = { "Tool Pin", "Tool Barb", "Curve Claw", "Curve Claw Upgraded", "Hero Shakra Ring", "Tool Bomb", "Hero Conch Projectile", "Tool Lightning Bola" };
                    if (!toPatcher.Contains(usage.ThrowPrefab.name)) {
                        return true;
                    }
                }
                AimingManager.LastClickTime = Time.time;
                AttackToolBinding? attackToolBinding = ToolItemManager.GetAttackToolBinding(willThrowTool);
                if (attackToolBinding == null) return true;
                inputHandler.inputActions.QuickCast.ClearInputState();

                bool isEmpty = willThrowTool.IsEmpty;

                // 检查是否可以投掷
                var canThrowMethod = AccessTools.Method(typeof(HeroController), "CanThrowTool", new Type[] {
                    typeof(ToolItem), typeof(AttackToolBinding), typeof(bool)
                });
                if (!(bool)canThrowMethod.Invoke(__instance, new object[] { willThrowTool, attackToolBinding.Value, true })) {
                    return false;
                }

                // 如果当前状态正在投掷工具，结束投掷
                if (cState.isToolThrowing) {
                    return true;
                }
                // 重置视角
                __instance.ResetLook();
                // 重置后坐力和漂浮状态
                cState.recoiling = false;
                cState.floating = false;
                // 取消攻击动作
                __instance.CancelAttack(true);
                // 如果不是自动投掷，尝试设置正确朝向且处于墙滑状态，则翻转角色精灵
                if (!isAutoThrow && __instance.TrySetCorrectFacing(true) && cState.wallSliding) {
                    __instance.FlipSprite();
                }
                // 标记工具已使用
                var DidUseAttackToolMethod = AccessTools.Method(typeof(HeroController), "DidUseAttackTool");
                DidUseAttackToolMethod.Invoke(__instance, new object[] { savedData });
                //__instance.DidUseAttackTool(savedData);
                // 设置投掷状态
                cState.isToolThrowing = true;
                cState.toolThrowCount++;
                cState.throwingToolVertical = willThrowTool.Usage.ThrowAnimVerticalDirection;
                Type instanceType = __instance.GetType();

                // 根据投掷方向获取动画时长
                instanceType.GetField("toolThrowDuration", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance,
                    (cState.throwingToolVertical > 0) ? animCtrl.GetClipDuration("ToolThrow Up") : animCtrl.GetClipDuration("ToolThrow Q"));
                instanceType.GetField("toolThrowTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, 0f);
                // 标记攻击动作已执行
                var DidAttackMethod = AccessTools.Method(typeof(HeroController), "DidAttack");
                DidAttackMethod.Invoke(__instance, new object[] { });
                // 设置投掷冷却时间
                instanceType.GetField("throwToolCooldown", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, usage.ThrowCooldown);
                // 播放投掷音效
                __instance.attackAudioTable.SpawnAndPlayOneShot(__instance.transform.position, false);
                // 如果有投掷预制体，实例化并设置物理参数
                if (usage.ThrowPrefab) {
                    Transform transform;
                    Vector2 vector;
                    float num;
                    float num2;
                    // 根据是否墙滑选择投掷点和方向
                    if (cState.wallSliding) {
                        transform = toolThrowWallPoint;
                        vector = Vector2.right;
                        num = -1f;
                        num2 = (float)(cState.facingRight ? 180 : 0);
                    }
                    else {
                        transform = toolThrowPoint;
                        vector = Vector2.left;
                        num = 1f;
                        num2 = (float)(cState.facingRight ? 0 : 180);
                    }

                    // 获取投掷点
                    Transform throwPoint = cState.wallSliding ? toolThrowWallPoint : toolThrowPoint;

                    // 如果有近距离投掷点，检查是否有障碍物
                    if (toolThrowClosePoint) {
                        Vector3 position = throwPoint.position;
                        Vector3 direction = toolThrowClosePoint.TransformDirection(Vector2.left);
                        Vector3 closePosition = toolThrowClosePoint.position;
                        float distance = Mathf.Abs(position.x - closePosition.x);

                        if (Physics2D.Raycast(closePosition, direction, distance, 8448)) {
                            throwPoint = toolThrowClosePoint;
                        }
                    }

                    // 计算投掷偏移
                    Vector2 throwOffset = (isAutoThrow && usage.UseAltForQuickSling) ?
                        usage.ThrowOffsetAlt : usage.ThrowOffset;

                    Debug.Log("facingRight :" + cState.facingRight);
                    if (cState.wallSliding) {
                        throwOffset.x += 0.5f;
                    }
                    //Debug.Log("-----------------offset :" + throwOffset);
                    // 添加随机扰动
                    throwOffset.y += UnityEngine.Random.Range(-0.1f, 0.1f);
                    Vector2 throwPointPos = throwPoint.TransformPoint(throwOffset);
                    //DebugLineRenderer.DrawLine(throwPointPos + Vector2.up * 5, throwPointPos + Vector2.up * -5, Color.green, 2);
                    // 实例化投掷物
                    GameObject throwObject = usage.ThrowPrefab.Spawn(throwPoint.TransformPoint(throwOffset));

                    // 计算朝向鼠标的方向
                    Vector2 throwDirection = (mouseWorldPos - throwPoint.position + throwOffset.ToVector3(0)).normalized;

                    //if (!cState.wallSliding) {

                    // 方向不对就转身
                    if ((cState.facingRight ? 1 : -1) * Mathf.Sign(throwDirection.x) < 0) {
                        __instance.FlipSprite();
                    }
                    //}

                    // 如果需要缩放，调整大小
                    if (usage.ScaleToHero) {
                        Vector3 localScale = throwObject.transform.localScale;
                        //localScale.x = Mathf.Sign(throwDirection.x) * Mathf.Abs(localScale.x);
                        //if (usage.FlipScale) localScale.x = -localScale.x;
                        throwObject.transform.localScale = localScale;

                        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
                        throwObject.transform.SetLocalRotation2D(angle);
                        float num4 = (cState.facingRight ? (-num) : num);
                        localScale.x = num4 * usage.ThrowPrefab.transform.localScale.x;
                        if (usage.FlipScale) {
                            localScale.x = -localScale.x;
                        }
                        // 设置伤害方向
                        if (usage.SetDamageDirection) {
                            DamageEnemies damage = throwObject.GetComponent<DamageEnemies>();
                            if (damage) {
                                // 计算朝向鼠标的角度
                                damage.SetDirection(angle);
                            }
                        }
                    }

                    // 计算投掷速度
                    Vector2 throwVelocity = (isAutoThrow && usage.UseAltForQuickSling) ?
                        usage.ThrowVelocityAlt : usage.ThrowVelocity;

                    // 应用鼠标方向
                    throwVelocity = throwVelocity.magnitude * throwDirection;

                    // 设置刚体速度
                    Rigidbody2D rb = throwObject.GetComponent<Rigidbody2D>();
                    if (rb) {
                        rb.linearVelocity = throwVelocity;
                    }
                    // 触发投掷震动反馈
                    vibrationCtrl.PlayToolThrow();
                }
                else {
                    Vector2 throwDirection = (mouseWorldPos - heroPos).normalized;

                    // 方向不对就转身
                    if (!cState.wallSliding) {
                        if ((cState.facingRight ? 1 : -1) * Mathf.Sign(throwDirection.x) < 0) {
                            __instance.FlipSprite();
                        }
                    }
                    bool test = false;
                    if (test) {

                        //toolEventTarget.SendEvent(usage.FsmEventName);
                        var event1 = FsmEvent.GetFsmEvent(usage.FsmEventName);
                        var fsm = toolEventTarget.Fsm;
                        //fsm.ProcessEvent(event1);
                        if (!fsm.Started) {
                            fsm.Start();
                        }
                        FsmExecutionStack.PushFsm(fsm);
                        //Debug.Log(fsm.ActiveState.Transitions.Length);
                        foreach (var trans in fsm.ActiveState.Transitions) {
                            if (trans.FsmEvent == event1) {
                                FsmState state = trans.ToFsmState;
                                //Debug.Log(state.Name);
                                fsm.SwitchState(state);
                                state = state.Transitions[0].ToFsmState;
                                //Debug.Log(state.Name);
                                //AccessTools.Field(typeof(Fsm), "activeState").SetValue(fsm, state);
                                //var EnterStateMethod = AccessTools.Method(typeof(Fsm), "EnterState");
                                //EnterStateMethod.Invoke(fsm, new object[] { state });
                                //fsm.StateChanged(state);
                                state.OnEnter();
                                //Debug.Log(state.Actions.Length);
                                AccessTools.Field(typeof(FsmState), "active").SetValue(state, true);
                                foreach (var action in state.Actions) {
                                    //Debug.Log(action);
                                    action.Init(state);
                                    action.OnEnter();
                                    action.OnExit();
                                    action.Finish();
                                    //action.Enabled = false;
                                    //action.IsOpen = false;
                                }
                                //state.Actions = new FsmStateAction[0];
                                //Debug.Log(state.Transitions.Length);

                                foreach (var _trans in state.Transitions) {
                                    //Debug.Log(_trans + " : " + _trans.EventName);
                                    FsmState _state = _trans.ToFsmState;
                                    if (_state != null) {

                                        foreach (var action in _state.Actions) {
                                            //Debug.Log(action);
                                        }
                                    }
                                }                            //state.OnExit();
                                                             //fsm.SwitchState(state);
                            }
                        }

                        FsmExecutionStack.PopFsm();

                    }
                    else {
                        toolEventTarget.SendEvent(usage.FsmEventName);
                    }
                    // guns 
                    if (!usage.IsNonBlockingEvent) {
                        //Debug.Log("-----IsNonBlockingEvent");
                        //if (cState.wallSliding) {
                        //    __instance.FlipSprite();
                        //    __instance.CancelWallsliding();
                        //}
                        test = false;
                        toolEventTarget.SendEventSafe("TAKE CONTROL");
                        string activeStateName = toolEventTarget.ActiveStateName;
                        if (test) {

                            //toolEventTarget.SendEvent(usage.FsmEventName);
                            var event1 = FsmEvent.GetFsmEvent(usage.FsmEventName);
                            var fsm = toolEventTarget.Fsm;
                            //fsm.ProcessEvent(event1);
                            if (!fsm.Started) {
                                fsm.Start();
                            }
                            FsmExecutionStack.PushFsm(fsm);
                            //Debug.Log(fsm.ActiveState.Transitions.Length);
                            foreach (var trans in fsm.ActiveState.Transitions) {
                                if (trans.FsmEvent == event1) {
                                    FsmState state = trans.ToFsmState;
                                    //Debug.Log(state.Name);
                                    fsm.SwitchState(state);
                                    state = state.Transitions[0].ToFsmState;
                                    //Debug.Log(state.Name);
                                    //AccessTools.Field(typeof(Fsm), "activeState").SetValue(fsm, state);
                                    //var EnterStateMethod = AccessTools.Method(typeof(Fsm), "EnterState");
                                    //EnterStateMethod.Invoke(fsm, new object[] { state });
                                    //fsm.StateChanged(state);
                                    state.OnEnter();
                                    Debug.Log(state.Actions.Length);
                                    AccessTools.Field(typeof(FsmState), "active").SetValue(state, true);
                                    foreach (var action in state.Actions) {
                                        Debug.Log(action);
                                        action.Init(state);
                                        action.OnEnter();
                                        action.OnExit();
                                        action.Finish();
                                        //action.Enabled = false;
                                        //action.IsOpen = false;
                                    }
                                    //state.Actions = new FsmStateAction[0];
                                    Debug.Log(state.Transitions.Length);

                                    foreach (var _trans in state.Transitions) {
                                        Debug.Log(_trans + " : " + _trans.EventName);
                                        FsmState _state = _trans.ToFsmState;
                                        if (_state != null) {

                                            foreach (var action in _state.Actions) {
                                                Debug.Log(action);
                                            }
                                        }
                                    }                            //state.OnExit();
                                                                 //fsm.SwitchState(state);
                                }
                            }

                            FsmExecutionStack.PopFsm();

                        }
                        else {
                            toolEventTarget.SendEvent(usage.FsmEventName);
                        }
                        if (toolEventTarget.ActiveStateName != activeStateName) {
                            DidUseAttackToolMethod.Invoke(__instance, new object[] { savedData });
                            return false;
                        }
                    }
                    //toolEventTarget.SendEventSafe(usage.FsmEventName);
                }
                // 通知工具已被使用
                var onWasUsedMethod = AccessTools.Method(typeof(ToolItem), "OnWasUsed");
                onWasUsedMethod.Invoke(willThrowTool, new object[] { isEmpty });

                // 判断是否装备了快速投掷工具且没有自定义工具覆盖
                bool flag2 = Gameplay.QuickSlingTool.Status.IsEquipped && !ToolItemManager.IsCustomToolOverride;
                if (flag2) {
                    // 播放快速投掷音效
                    __instance.quickSlingAudioTable.SpawnAndPlayOneShot(__instance.transform.position, false);
                }
                // 如果不是自动投掷且满足快速投掷条件且可以继续投掷，设置排队自动投掷标志
                if (!isAutoThrow && flag2) {
                    instanceType.GetField("queuedAutoThrowTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, true);
                    return false;
                }
                // 清空准备投掷工具和排队标志
                instanceType.GetField("willThrowTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, null);
                instanceType.GetField("queuedAutoThrowTool", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, false);
                return false; // 跳过原始方法
            }
            catch (Exception ex) {
                Debug.LogError($"ThrowTool patch failed: {ex}");
                return true; // 执行原始方法
            }
        }
    }
}
