using NUnit.Framework;

namespace WarWW3.Tests
{
    public sealed class DomainTests
    {
        [TestCase("movement_reaches_destination")]
        [TestCase("path_avoids_buildings")]
        [TestCase("unreachable_path_rejected")]
        [TestCase("invalid_commands_do_not_mutate")]
        [TestCase("friendly_fire_rejected")]
        [TestCase("armor_damage_floor")]
        [TestCase("line_of_sight_blocks_damage")]
        [TestCase("simultaneous_fire_draw")]
        [TestCase("victory_latches_and_freezes")]
        [TestCase("defeat_detected")]
        [TestCase("ai_completes_skirmish")]
        [TestCase("replay_hash_is_repeatable")]
        [TestCase("invalid_unit_stats_rejected")]
        [TestCase("hold_cancels_movement")]
        [TestCase("grid_rejects_nonfinite_positions")]
        [TestCase("attack_tracks_moving_target")]
        public void DomainRule(string name) { CoreChecks.Cases()[name](); }
    }
}
