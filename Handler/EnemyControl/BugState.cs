namespace LethalMenu.Handler.EnemyControl;

/// <summary>HoarderBugAI.currentBehaviourStateIndex values (must match the game).</summary>
internal enum BugState
{
	/// <summary>Wandering / looking for items (also its idle state).</summary>
	SEARCHING_FOR_ITEMS,
	RETURNING_TO_NEST,
	/// <summary>Angry chase: the stand-up "flying" pose; hurts players on contact.</summary>
	CHASING_PLAYER
}
