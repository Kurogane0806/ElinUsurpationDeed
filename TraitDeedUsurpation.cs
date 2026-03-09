using System.Collections.Generic;

namespace UsurpationDeed;

/// <summary>
/// Trait for the "Land Usurpation Deed" item.
///
/// Must be used while INSIDE a Zone_User (a player-published base entered via Moongate).
/// User zones only exist inside the game world when visited through a Moongate/Stargate —
/// they are not accessible directly from the world map.
///
/// Flow:
///   1. Player enters a Zone_User via Moongate.
///   2. Player uses this deed from their inventory while inside the zone.
///   3. The deed reads the current zone's world-map coordinates (zone.x / zone.y).
///   4. Player picks which of their home branches to relocate.
///   5. On confirm: exits back to world map, destroys the Zone_User,
///      and places the chosen branch at those same elomap coordinates.
/// </summary>
public class TraitDeedUsurpation : TraitScroll
{
    // Lang keys defined in package/LangMod/EN/Source.xlsx (sheet: Note)
    private const string KEY_NOT_USER_ZONE = "usurp_notUserZone";
    private const string KEY_OWN_ZONE      = "cannot_use_here";
    private const string KEY_CONFIRM       = "usurp_confirm";
    private const string KEY_SUCCESS       = "usurp_success";

    public override bool CanBeDestroyed => false;

    public override void OnRead(Chara c)
    {
        // 1. Must be INSIDE a Zone_User (entered via Moongate)
        //    Zone_User is the class for all player-published maps.
        if (EClass._zone is not Zone_User targetZone)
        {
            Msg.Say(KEY_NOT_USER_ZONE);
            return;
        }

        // 2. Cannot usurp your own faction zone
        if (targetZone.IsPCFaction)
        {
            Msg.Say(KEY_OWN_ZONE);
            return;
        }

        Plugin.ModLog($"Target zone: {targetZone.Name} at elomap ({targetZone.x}, {targetZone.y})");

        // 3. Show list of player's home branches to pick which one to relocate here
        List<FactionBranch> children = EClass.pc.faction.GetChildren();
        string targetZoneName = targetZone.Name;
        int targetX = targetZone.x;
        int targetY = targetZone.y;

        EClass.ui
            .AddLayer<LayerList>()
            .SetNoSound()
            .SetList2(
                children,
                (FactionBranch b) => b.owner.NameWithLevel,
                delegate(FactionBranch chosen, ItemGeneral s)
                {
                    Zone branchZone = chosen.owner;

                    // 4. Confirmation dialog with flavourful usurpation text
                    Dialog.YesNo(KEY_CONFIRM, delegate
                    {
                        Plugin.ModLog($"Usurping '{targetZoneName}' ({targetX},{targetY}) for branch '{branchZone.Name}'");

                        // Consume one charge before anything else
                        owner.ModNum(-1);

                        // Exit the Zone_User and return to world map first,
                        // then destroy + relocate once we're safely outside.
                        // We use a ReturnCallback so the zone exit happens cleanly.
                        EClass.pc.ReturnZone(delegate
                        {
                            // --- Now on the world map ---

                            // Destroy the foreign Zone_User at its elomap cell
                            EClass.scene.elomap.SetZone(targetX, targetY, null);
                            targetZone.Destroy();

                            // Remove branch from its old elomap position
                            EClass.scene.elomap.SetZone(branchZone.x, branchZone.y, null);

                            // Place branch at the usurped coordinates
                            branchZone.x = targetX;
                            branchZone.y = targetY;

                            EClass.scene.elomap.SetZone(
                                branchZone.x, branchZone.y, branchZone, updateMesh: true);

                            EClass.game.Save();
                            EClass.Sound.Play("jingle_embark");
                            EClass.pc.PlaySound("build");

                            Msg.Say(KEY_SUCCESS, branchZone.Name, targetZoneName);
                        });
                    });
                },
                null
            )
            .TryShowHint("h_relocate");
    }
}
