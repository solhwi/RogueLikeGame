using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CharacterState
{
	Idle, // 서있기 (노무기, 한손일반무기)
	Run, // 뛰기 (노무기, 한손일반무기)

	Idle_TwinHand, // 서있기 (쌍검)
	Run_TwinHand, // 뛰기 (쌍검)
	Idle_TwoHand, // 서있기 (두손무기)
	Run_TwoHand, // 뛰기 (두손무기)
	Idle_Net, // 서있기 (잠자리채)
	Run_Net, // 뛰기 (잠자리채)
	Idle_Umbrella, // 서있기 (우산)
	Run_Umbrella, // 뛰기 (우산)
	Idle_GreatSword, // 서있기 (그레이트소드)
	Run_GreatSword, // 뛰기 (그레이트소드)

	Attack_GreatSword, // 공격 (그레이트소드)
	Attack_TwinHand, // 공격 (쌍검)

	Clap, // 박수치기
	LookAround, // 둘러보기
	BarricadeDown,      // 바리케이드에 맞고 넘어짐

	DoubleDiceBuff, // 주사위 두배 버프 획득
	HalfDiceBuff,   // 주사위 반감 디버프 획득
	MinusDiceBuff,  // 주사위 마이너스 디버프 획득
	DrunkBuff,          // 1 또는 6 버프 획득
	OddBuff,            // 홀수 버프 획득
	EvenBuff,           // 짝수 버프 획득
}

public enum CharacterStateType
{
	Idle,
	Run,
	Attack,
	LookAround,
	Clap,
	DropItem
}

public class CharacterAnimatorComponent : MonoBehaviour
{
	[SerializeField] private CharacterEquipmentInventory inventory = null;

	public Animator Animator
	{
		get
		{
			if (animator == null)
			{
				animator = GetComponentInChildren<Animator>();
			}

			return animator;
		}
	}

	private Animator animator = null;

	private CharacterStateType currentStateType = CharacterStateType.Idle;
	public CharacterState currentState = CharacterState.Idle;

	public void DoIdle(float crossFadeTime = 0.0f)
	{
		currentStateType = CharacterStateType.Idle;

		var propType = inventory.GetEquippedPropType().FirstOrDefault();
		if (propType == PropType.GreatSword)
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle_GreatSword, crossFadeTime);
		}
		else if (propType == PropType.Net)
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle_Net, crossFadeTime);
		}
		else if (propType == PropType.Umbrella)
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle_Umbrella, crossFadeTime);
		}
		else if (propType == PropType.TwinDagger_L || propType == PropType.TwinDagger_R)
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle_TwinHand, crossFadeTime);
		}
		else if (propType == PropType.Axe || propType == PropType.PickAx || propType == PropType.Shovel)
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle_TwoHand, crossFadeTime);
		}
		else
		{
			ChangeState(CharacterStateType.Idle, CharacterState.Idle, crossFadeTime);
		}
	}

	public void DoRun(float crossFadeTime = 0.0f)
	{
		currentStateType = CharacterStateType.Run;

		var propType = inventory.GetEquippedPropType().FirstOrDefault();
		if (propType == PropType.GreatSword)
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run_GreatSword, crossFadeTime);
		}
		else if (propType == PropType.Net)
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run_Net, crossFadeTime);
		}
		else if (propType == PropType.Umbrella)
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run_Umbrella, crossFadeTime);
		}
		else if (propType == PropType.TwinDagger_L || propType == PropType.TwinDagger_R)
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run_TwinHand, crossFadeTime);
		}
		else if (propType == PropType.Axe || propType == PropType.PickAx || propType == PropType.Shovel)
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run_TwoHand, crossFadeTime);
		}
		else
		{
			ChangeState(CharacterStateType.Run, CharacterState.Run, crossFadeTime);
		}
	}

	public void DoAttack(float crossFadeTime = 0.0f)
	{
		currentStateType = CharacterStateType.Attack;

		var propType = inventory.GetEquippedPropType().FirstOrDefault();
		if (propType == PropType.GreatSword)
		{
			ChangeState(CharacterStateType.Attack, CharacterState.Attack_GreatSword, crossFadeTime);
		}
		else if (propType == PropType.TwinDagger_L || propType == PropType.TwinDagger_R)
		{
			ChangeState(CharacterStateType.Attack, CharacterState.Attack_TwinHand, crossFadeTime);
		}
	}

	public void ChangeState(CharacterStateType stateType, CharacterState state, float crossFadeTime = 0.0f)
	{
		currentStateType = stateType;
		currentState = state;

		if (Animator != null)
		{
			Animator.CrossFade(state.ToString(), crossFadeTime);
		}
	}

	public void Replay()
	{
		switch (currentStateType)
		{
			case CharacterStateType.Idle:
				DoIdle();
				break;

			case CharacterStateType.Run:
				DoRun();
				break;

			case CharacterStateType.Attack:
				DoAttack();
				break;

			default:
				ChangeState(currentStateType, currentState);
				break;
		}
	}
}
