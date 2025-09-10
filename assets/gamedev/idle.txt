using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : StateMachineBehaviour
{
    [SerializeField]
    private float _timeUntilIdle;

    [SerializeField]
    private int _numberOfIdleAnimation;

    private bool _isIdle;
    private float _idleTime;
    private int _idleLoop;



    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ResetIdle();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_isIdle == false)
        {
            _idleTime += Time.deltaTime;

            if (_idleTime > _timeUntilIdle && stateInfo.normalizedTime % 1 < 0.02f)

            { 
                _isIdle = true;
                _idleLoop = Random.Range(1, _numberOfIdleAnimation + 1);
                _idleLoop = _idleLoop * 2 - 1;

                animator.SetFloat("idleLoop", _idleLoop - 1);
            
            }
        }
        else if (stateInfo.normalizedTime % 1 > 0.98) 
        {
            ResetIdle();
        }

        animator.SetFloat("idleLoop", _idleLoop, 0.2f, Time.deltaTime);
    }

    private void ResetIdle()
    {
        if (_isIdle)
        {
            _idleLoop--;
        }
        _isIdle = false;
        _idleTime = 0;
        
        
    }


    
}
