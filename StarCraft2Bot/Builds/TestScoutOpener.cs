using SC2APIProtocol;
using Sharky;
using Sharky.Builds;
using Sharky.Chat;
using Sharky.DefaultBot;
using Sharky.Managers;
using Sharky.MicroControllers;
using Sharky.MicroTasks;
using Sharky.MicroTasks.Attack;
using Sharky.Proxy;
using StarCraft2Bot.Bot;
using StarCraft2Bot.Builds.Base;
using StarCraft2Bot.Builds.Base.Condition;
using StarCraft2Bot.Builds.Base.Desires;
using StarCraft2Bot.Helper;

namespace StarCraft2Bot.Builds
{
    public class TestScoutOpener : Build
    {
        private EnemyInformationsManager EnemyInformationsManager;
        private EnemyUnitMemoryService UnitMemoryService;
        private EnemyUnitApproximationService EnemyUnitApproximationService;

        private Queue<BuildAction>? BuildOrder { get; set; }

        public TestScoutOpener(BaseBot defaultSharkyBot)
            : base(defaultSharkyBot)
        {
            defaultSharkyBot.MicroController = new AdvancedMicroController(defaultSharkyBot);
            var advancedAttackTask = new AdvancedAttackTask(
                defaultSharkyBot,
                new EnemyCleanupService(
                    defaultSharkyBot.MicroController,
                    defaultSharkyBot.DamageService
                ),
                new List<UnitTypes> { UnitTypes.TERRAN_MARINE },
                100f,
                true
            );
            defaultSharkyBot.MicroTaskData[typeof(AttackTask).Name] = advancedAttackTask;

            UnitMemoryService = defaultSharkyBot.EnemyUnitMemoryService;
            EnemyUnitApproximationService = defaultSharkyBot.EnemyUnitApproximationService;

            EnemyInformationsManager = new EnemyInformationsManager(
                UnitCountService,
                MapDataService,
                ActiveUnitData,
                UnitMemoryService,
                defaultSharkyBot.SharkyUnitData,
                FrameToTimeConverter,
                defaultSharkyBot.MapMemoryService,
                defaultSharkyBot.EnemyUnitApproximationService
            );
        }

        public override void StartBuild(int frame)
        {
            base.StartBuild(frame);

            BuildOptions.StrictGasCount = true;
            BuildOptions.StrictSupplyCount = false;
            BuildOptions.StrictWorkerCount = false;

            MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 90;
            MacroData.DesiredUnitCounts[UnitTypes.TERRAN_BARRACKS] = 1;
            // MacroData.DesiredMorphCounts[UnitTypes.TERRAN_ORBITALCOMMAND] = 4;

            //MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 5;

            BuildOrder = new Queue<BuildAction>();

            MicroTaskData[typeof(WorkerScoutTask).Name].Enable();

            BuildOrder.Enqueue(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_SCV, 16, UnitCountService),
                    new ProductionStructureDesire(UnitTypes.TERRAN_COMMANDCENTER, 2, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new UnitCountCondition(UnitTypes.TERRAN_COMMANDCENTER, 2, UnitCountService),
                    new CustomDesire(() =>
                    {
                        BuildOptions.StrictGasCount = false;
                    })
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_SCV, 32, UnitCountService),
                    new ProductionStructureDesire(UnitTypes.TERRAN_COMMANDCENTER, 3, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_SCV, 48, UnitCountService),
                    new ProductionStructureDesire(UnitTypes.TERRAN_COMMANDCENTER, 4, MacroData)
                )
            );
            BuildOrder.Enqueue(
                new BuildAction(
                    new UnitCompletedCountCondition(UnitTypes.TERRAN_SCV, 64, UnitCountService),
                    new ProductionStructureDesire(UnitTypes.TERRAN_COMMANDCENTER, 5, MacroData)
                )
            );
            //foreach (UnitCommander commander in MicroTaskData[typeof(Micro).Name])
            //{
            //    if (commander.CommanderState == CommanderState.None)
            //    {
            //        commander.Claimed = false;
            //        MicroTaskData[typeof(WorkerScoutTask).Name].StealUnit(commander);
            //    }
            //}
        }

        public override void OnFrame(ResponseObservation observation)
        {
            //base.OnFrame(observation);
            //if (BuildOrder == null)
            //{
            //    throw new InvalidOperationException("BuildOrder has not been initialized.");
            //}

            //if (BuildOrder.Count == 0)
            //{
            //    return;
            //}

            //if (new TimeCondition(25.0).IsFulfilled(observation.Observation))
            //{
            //    proxyTask.DesiredWorkers = 2;
            //}


            Console.WriteLine("Frame: " + observation.Observation.GameLoop + "\n======");
            Console.WriteLine(
                "Mineralapproximation: "
                    + EnemyInformationsManager.GetApproximatedAverageProducedEnemyMinerals(
                        observation
                    )
            );

            Console.WriteLine("Seen:\n=====");
            foreach (var key in UnitMemoryService.CurrentTotalUnits.Keys)
            {
                Console.WriteLine(UnitMemoryService.CurrentTotalUnits[key] + "x " + key.ToString());
            }

            // Console.WriteLine("Approximated SCVs:\n=============");
            // foreach (var item in EnemyUnitApproximationService.LastTotalUnits[UnitTypes.TERRAN_SCV])
            // {
            //     Console.WriteLine(item.Key + ": " + item.Value + "x");
            // }
            // Console.WriteLine("Approximated CCs:\n=============");
            // foreach (
            //     var item in EnemyUnitApproximationService.LastTotalUnits[
            //         UnitTypes.TERRAN_COMMANDCENTER
            //     ]
            // )
            // {
            //     Console.WriteLine(item.Key + ": " + item.Value + "x");
            // }

            // Console.WriteLine("Approximated Army:\n=============");

            // foreach (var key in approx.Keys)
            // {
            //     Console.WriteLine(approx[key] + "x " + key.ToString());
            // }

            // Console.WriteLine(EnemyInformationsManager.GetVisibleAreaPercentage() + "%");

            if (BuildOrder.Count == 0)
            {
                return;
            }

            var nextAction = BuildOrder.Peek();

            if (nextAction.AreConditionsFulfilled())
            {
                nextAction.EnforceDesires();
                BuildOrder.Dequeue();
            }
        }

        public override bool Transition(int frame)
        {
            //if (BuildOrder == null)
            //{
            //    throw new InvalidOperationException("BuildOrder has not been initialized.");
            //}

            //if (BuildOrder.Count == 0)
            //{
            //    //MacroData.DesiredUnitCounts[UnitTypes.TERRAN_SCV] = 12;
            //    //MacroData.DesiredUnitCounts[UnitTypes.TERRAN_REAPER] = 0;

            //    //AttackData.UseAttackDataManager = true;
            //    //proxyTask.Disable();
            //    //return true;
            //}

            //return base.Transition(frame);
            return false;
        }
    }
}
