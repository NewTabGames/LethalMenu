namespace LethalMenu.Handler.EnemyControl;

internal interface IController
{
	const float DefaultSprintMultiplier = 2.8f;

	const float DefaultInteractRange = 2.5f;

	void OnTakeControl(EnemyAI enemy);

	void OnReleaseControl(EnemyAI enemy);

	void OnDeath(EnemyAI enemy);

	void Update(EnemyAI enemy, bool isAIControlled);

	void UsePrimarySkill(EnemyAI enemy);

	void OnSecondarySkillHold(EnemyAI enemy);

	void UseSecondarySkill(EnemyAI enemy);

	void ReleaseSecondarySkill(EnemyAI enemy);

	void OnMovement(EnemyAI enemy, bool isMoving, bool isSprinting);

	bool IsAbleToMove(EnemyAI enemy);

	bool IsAbleToRotate(EnemyAI enemy);

	bool CanUseEntranceDoors(EnemyAI enemy);

	string? GetPrimarySkillName(EnemyAI enemy);

	string? GetSecondarySkillName(EnemyAI enemy);

	float InteractRange(EnemyAI enemy);

	float SprintMultiplier(EnemyAI enemy);

	bool SyncAnimationSpeedEnabled(EnemyAI enemy);
}
