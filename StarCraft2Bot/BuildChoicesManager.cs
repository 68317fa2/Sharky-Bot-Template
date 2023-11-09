﻿using Sharky.DefaultBot;
using Sharky.MicroControllers;
using Sharky;
using SC2APIProtocol;
using Sharky.Builds;
using StarCraft2Bot.Builds;

namespace StarCraft2Bot
{
    public class BuildChoicesManager
    {
        private DefaultSharkyBot defaultSharkyBot = null!;
        private IndividualMicroController scvMicroController = null!;

        public BuildChoicesManager(DefaultSharkyBot newDefaultSharkyBot)
        {
            defaultSharkyBot = newDefaultSharkyBot;
            scvMicroController = new IndividualMicroController(newDefaultSharkyBot, newDefaultSharkyBot.SharkyAdvancedPathFinder, MicroPriority.JustLive, false);
        }

        public BuildChoices GetBuildChoices()
        {

            var reaperCheese = new ReaperOpener(defaultSharkyBot, scvMicroController);
            var saltyMarines = new SaltyMarines(defaultSharkyBot, scvMicroController);
            var tvTOpener = new TvTOpener(defaultSharkyBot, scvMicroController);

            var builds = new Dictionary<string, ISharkyBuild>
            {
                [reaperCheese.Name()] = reaperCheese,
                [saltyMarines.Name()] = saltyMarines,
                [tvTOpener.Name()] = tvTOpener,
            };
            var transitions = new List<List<string>>
            {
                new List<string> { saltyMarines.Name() },
            };

            var defaultSequences = new List<List<string>>
            {
                new List<string> {
                    tvTOpener.Name(),
                },
            };

            var cheeseSequences = new List<List<string>>
            {
                 new List<string> {
                    reaperCheese.Name(),
                },
            };

            // INFO: The "Transition" entry should usually contain something other than the same builds over again
            var buildSequences = new Dictionary<string, List<List<string>>>
            {
                [Race.Terran.ToString()] = cheeseSequences,
                ["Transition"] = transitions
            };

            return new BuildChoices { Builds = builds, BuildSequences = buildSequences };
        }
    }
}
