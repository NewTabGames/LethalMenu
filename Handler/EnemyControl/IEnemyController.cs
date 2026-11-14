namespace LethalMenu.Handler.EnemyControl;

internal interface IEnemyController<T> : IController where T : EnemyAI
{
	void OnTakeControl(T enemy)
	{
	}

	void OnReleaseControl(T enemy)
	{
	}

	void OnDeath(T enemy)
	{
	}

	void Update(T enemy, bool isAIControlled)
	{
	}

	void UsePrimarySkill(T enemy)
	{
	}

	void OnSecondarySkillHold(T enemy)
	{
	}

	void UseSecondarySkill(T enemy)
	{
	}

	void ReleaseSecondarySkill(T enemy)
	{
	}

	void OnMovement(T enemy, bool isMoving, bool isSprinting)
	{
	}

	bool IsAbleToMove(T enemy)
	{
		return true;
	}

	bool IsAbleToRotate(T enemy)
	{
		return true;
	}

	bool CanUseEntranceDoors(T enemy)
	{
		return true;
	}

	string? GetPrimarySkillName(T enemy)
	{
		return null;
	}

	string? GetSecondarySkillName(T enemy)
	{
		return null;
	}

	float InteractRange(T enemy)
	{
		return 2.5f;
	}

	float SprintMultiplier(T enemy)
	{
		return 2.8f;
	}

	bool SyncAnimationSpeedEnabled(T enemy)
	{
		return true;
	}

	void IController.OnTakeControl(EnemyAI enemy)
	{
		OnTakeControl((T)enemy);
	}

	void IController.OnReleaseControl(EnemyAI enemy)
	{
		OnReleaseControl((T)enemy);
	}

	void IController.OnDeath(EnemyAI enemy)
	{
		OnDeath((T)enemy);
	}

	void IController.Update(EnemyAI enemy, bool isAIControlled)
	{
		Update((T)enemy, isAIControlled);
	}

	void IController.UsePrimarySkill(EnemyAI enemy)
	{
		UsePrimarySkill((T)enemy);
	}

	void IController.OnSecondarySkillHold(EnemyAI enemy)
	{
		OnSecondarySkillHold((T)enemy);
	}

	void IController.UseSecondarySkill(EnemyAI enemy)
	{
		UseSecondarySkill((T)enemy);
	}

	void IController.ReleaseSecondarySkill(EnemyAI enemy)
	{
		ReleaseSecondarySkill((T)enemy);
	}

	void IController.OnMovement(EnemyAI enemy, bool isMoving, bool isSprinting)
	{
		OnMovement((T)enemy, isMoving, isSprinting);
	}

	bool IController.IsAbleToMove(EnemyAI enemy)
	{
		return IsAbleToMove((T)enemy);
	}

	bool IController.IsAbleToRotate(EnemyAI enemy)
	{
		return IsAbleToRotate((T)enemy);
	}

	bool IController.CanUseEntranceDoors(EnemyAI enemy)
	{
		return CanUseEntranceDoors((T)enemy);
	}

	string? IController.GetPrimarySkillName(EnemyAI enemy)
	{
		return GetPrimarySkillName((T)enemy);
	}

	string? IController.GetSecondarySkillName(EnemyAI enemy)
	{
		return GetSecondarySkillName((T)enemy);
	}

	float IController.InteractRange(EnemyAI enemy)
	{
		return InteractRange((T)enemy);
	}

	float IController.SprintMultiplier(EnemyAI enemy)
	{
		return SprintMultiplier((T)enemy);
	}

	bool IController.SyncAnimationSpeedEnabled(EnemyAI enemy)
	{
		return SyncAnimationSpeedEnabled((T)enemy);
	}
}
