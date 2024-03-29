using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.Managers;
using Sharky.MicroControllers;
using Sharky.MicroTasks;
using Sharky.MicroTasks.Attack;
using Sharky.Proxy;
using StarCraft2Bot.Bot;
using StarCraft2Bot.Builds.Base;
using StarCraft2Bot.Builds.Base.Condition;
using StarCraft2Bot.Builds.Base.Desires;

namespace StarCraft2Bot.Builds
{
    public class ReaperOpener : Build
    {
        private readonly ProxyLocationService proxyLocationService;
        private readonly ProxyTask proxyTask;

        private Queue<BuildAction>? BuildOrder { get; set; }

        public ReaperOpener(BaseBot defaultSharkyBot, IIndividualMicroController scvMicroController)
            : base(defaultSharkyBot)
        {
            proxyLocationService = defaultSharkyBot.ProxyLocationService;
            proxyTask = new ProxyTask(
                defaultSharkyBot,
                false,
                0.9f,
                string.Empty,
                scvMicroController
            )
            {
                ProxyName = nameof(ReaperOpener)
            };

            defaultSharkyBot.MicroController = new AdvancedMicroController(defaultSharkyBot);

            var advancedAttackTask = new AdvancedAttackTask(
                defaultSharkyBot,
                new EnemyCleanupService(
                    defaultSharkyBot.MicroController,
                    defaultSharkyBot.DamageService
                ),
                new List<UnitTypes> { UnitTypes.TERRAN_REAPER, UnitTypes.TERRAN_MARINE },
                2f,
                true
            );
            defaultSharkyBot.MicroTaskData[nameof(AttackTask)] = advancedAttackTask;

            var advancedAttackService = new AdvancedAttackService(
                defaultSharkyBot,
                advancedAttackTask
            );
            var advancedAttackDataManager = new AdvancedAttackDataManager(
                defaultSharkyBot,
                advancedAttackService,
                advancedAttackTask
            );
            defaultSharkyBot.AttackDataManager = advancedAttackDataManager;
            defaultSharkyBot.Managers.RemoveAll(m => m.GetType() == typeof(AttackDataManager));
            defaultSharkyBot.Managers.Add(advancedAttackDataManager);
        }

        /*
  12	  0:01	  SCV
  13	  0:13	  SCV
  14	  0:18	  Supply Depot
  14	  0:25	  SCV
  15	  0:41	  Barracks
  15	  0:45	  Refinery
  16	  1:00	  Barracks
  16	  1:01	  SCV
  16	  1:13	  SCV
  17	  1:29	  Reaper
  17	  1:30	  Barracks
  18	  1:33	  Refinery
  18	  1:36	  SCV
  19	  1:50	  Reaper
  20	  1:55	  Orbital Command
  20	  2:01	  Reaper
  21	  2:17	  Reaper
  21	  2:19	  Supply Depot
  21	  2:22	  Reaper	  x
  23	  2:40	  Reaper
  23	  2:49	  Reaper
  23	  2:50	  SCV
  24	  2:56	  Reaper
  25	  3:12	  Reaper
  25	  3:13	  SCV
  25	  3:19	  Factory
  26	  3:25	  SCV
  25	  3:38	  Reaper
  25	  3:39	  SCV
  28	  3:58	  Supply Depot
  28	  4:02	  Factory Tech Lab
  29	  4:21	  Command Center
        */

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            AttackData.CustomAttackFunction = true;
            AttackData.UseAttackDataManager = false;
            AttackData.AttackTrigger = 5f;
            AttackData.RetreatTrigger = 1f;
            AttackData.GroupUpEnabled = true;
            AttackData.KillTrigger = 5f;

            BuildOptions.StrictGasCount = true;
            BuildOptions.StrictSupplyCount = true;
            BuildOptions.StrictWorkerCount = true;

            MicroTaskData[GetType().Name] = proxyTask;
            proxyTask.DesiredWorkers = 1;
            var proxyLocation = proxyLocationService.GetCliffProxyLocation();
            MacroData.Proxies[proxyTask.ProxyName] = new ProxyData(proxyLocation, MacroData);
            proxyTask.Enable();

            BuildOrder = new Queue<BuildAction>();
            MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 12;
            MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 5;

            // Cancel-Conditions
            AddAction(
                new BuildAction(
                    new List<ICondition>
                    {
                        new EnemyHasUnitTypeCondition(UnitTypes.TERRAN_MARAUDER, UnitCountService),
                        new UnitCountCondition(
                            UnitTypes.TERRAN_REAPER,
                            5,
                            UnitCountService,
                            ConditionOperator.Smaller
                        )
                    },
                    new CustomDesire(() =>
                    {
                        ChatService.SendChatType($"{nameof(ReaperOpener)}-CANCEL");
                        AttackData.Attacking = false;
                    })
                )
            );
            AddAction(
                new BuildAction(
                    new List<ICondition>
                    {
                        new EnemyUnitCountCondition(
                            UnitTypes.TERRAN_BARRACKS,
                            2,
                            UnitCountService,
                            ConditionOperator.GreaterOrEqual
                        ),
                        new UnitCountCondition(
                            UnitTypes.TERRAN_REAPER,
                            5,
                            UnitCountService,
                            ConditionOperator.Smaller
                        )
                    },
                    new CustomDesire(() =>
                    {
                        ChatService.SendChatType($"{nameof(ReaperOpener)}-CANCEL");
                        AttackData.Attacking = false;
                    })
                )
            );

            // Normal Actions:
            //AddAction(new BuildAction(new UnitCompletedCountCondition(UnitTypes.TERRAN_REAPER, 1, UnitCountService),
            //             new CustomDesire(() =>
            //             {
            //                 MicroTaskData[typeof(ReaperWorkerHarassTask).Name].Enable();
            //             })));


            AddAction(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_BARRACKS, 1, UnitCountService),
                    new CustomDesire(() =>
                    {
                        proxyTask.Disable();
                        //MicroTaskData[typeof(WorkerScoutTask).Name].Enable();


                        //foreach (UnitCommander commander in proxyTask.UnitCommanders)
                        //{
                        //    if (commander.CommanderState == CommanderState.None)
                        //    {
                        //        commander.Claimed = false;
                        //        MicroTaskData[typeof(WorkerScoutTask).Name].StealUnit(commander);
                        //    }
                        //}
                    })
                )
            );

            AddAction(
                new BuildAction(
                    new UnitCountCondition(UnitTypes.TERRAN_BARRACKS, 2, UnitCountService),
                    new CustomDesire(() =>
                    {
                        MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 21;
                        MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 10;
                    })
                )
            );

            AddAction(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_BARRACKS, 2, UnitCountService),
                    new CustomDesire(() => {
                        //MicroTaskData[typeof(ReaperWorkerHarassTask).Name].Disable();
                    })
                )
            );

            AddAction(
                new BuildAction(
                    new UnitCountCondition(UnitTypes.TERRAN_BARRACKS, 1, UnitCountService),
                    new CustomDesire(() =>
                    {
                        MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 15;
                    })
                )
            );

            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(12, MacroData),
                    new SupplyDepotDesire(1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(12, MacroData),
                    new ProxyProductionStructureDesire(
                        UnitTypes.TERRAN_BARRACKS,
                        1,
                        MacroData,
                        proxyTask.ProxyName
                    )
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(13, MacroData),
                    new GasBuildingCountDesire(1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(14, MacroData),
                    new ProxyProductionStructureDesire(
                        UnitTypes.TERRAN_BARRACKS,
                        2,
                        MacroData,
                        proxyTask.ProxyName
                    )
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(17, MacroData),
                    new ProductionStructureDesire(UnitTypes.TERRAN_BARRACKS, 3, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(18, MacroData),
                    new GasBuildingCountDesire(2, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(20, MacroData),
                    new MorphDesire(UnitTypes.TERRAN_ORBITALCOMMAND, 1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(21, MacroData),
                    new SupplyDepotDesire(2, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(25, MacroData),
                    new ProductionStructureDesire(UnitTypes.TERRAN_FACTORY, 1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(28, MacroData),
                    new SupplyDepotDesire(3, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(28, MacroData),
                    new AddonStructureDesire(UnitTypes.TERRAN_FACTORYTECHLAB, 1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(29, MacroData),
                    new ProductionStructureDesire(UnitTypes.TERRAN_COMMANDCENTER, 1, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new SupplyCondition(29, MacroData),
                    new ProductionStructureDesire(UnitTypes.TERRAN_ENGINEERINGBAY, 2, MacroData)
                )
            );
        }

        public override void OnFrame(ResponseObservation observation)
        {
            base.OnFrame(observation);
            if (BuildOrder == null)
            {
                throw new InvalidOperationException("BuildOrder has not been initialized.");
            }

            if (BuildOrder.Count == 0)
            {
                return;
            }

            if (new TimeCondition(25.0).IsFulfilled(observation.Observation))
            {
                proxyTask.DesiredWorkers = 2;
            }

            var nextAction = BuildOrder.Peek();

            if (nextAction.AreConditionsFulfilled())
            {
                nextAction.EnforceDesires();
                BuildOrder.Dequeue();
            }

            ManageAttack();
        }

        private double GetEnemyReaperRateCount()
        {
            return UnitCountService.EquivalentEnemyTypeCount(UnitTypes.TERRAN_MARINE) * 1.25
                + UnitCountService.EquivalentEnemyTypeCount(UnitTypes.TERRAN_MARAUDER) * 5
                + UnitCountService.EquivalentEnemyTypeCount(UnitTypes.TERRAN_HELLION) * 3;
        }

        private void ManageAttack()
        {
            if (
                UnitCountService.EquivalentTypeCount(UnitTypes.TERRAN_REAPER)
                    >= GetEnemyReaperRateCount()
                || GetEnemyReaperRateCount() >= 0
            )
            {
                AttackData.Attacking = true;
            }
            else
            {
                AttackData.Attacking = false;
            }
        }

        public override bool Transition(int frame)
        {
            if (BuildOrder == null)
            {
                throw new InvalidOperationException("BuildOrder has not been initialized.");
            }

            if (BuildOrder.Count == 0)
            {
                MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 12;
                MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 0;

                AttackData.UseAttackDataManager = true;
                proxyTask.Disable();
                return true;
            }

            return base.Transition(frame);
        }
    }
}
