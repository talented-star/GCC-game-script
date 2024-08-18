
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GrabCoin.AIBehaviour
{
    [System.Serializable]
    public class AlarmList
    {
        private IBattleEnemy _enemy;
        private EnemyVision _vision;
        [SerializeField] private List<EnemyPriority> _alarmCharacters = new();
        private Optionals<IBattleEnemy> _targetEnemy = new();
        private bool _isAlarm;

        public bool IsAlarm => _isAlarm;

        public AlarmList(IBattleEnemy enemy, EnemyVision vision)
        {
            _vision = vision;
            _enemy = enemy;
            _alarmCharacters = new();
        }

        public Optionals<IBattleEnemy> GetTargetEnemy
        {
            get
            {
                if (!_targetEnemy.isInit)
                    UpdateAlarmEnemy();
                return _targetEnemy;
            }
        }

        public void ClearAlarm()
        {
            _isAlarm = false;
            _alarmCharacters.Clear();
            _targetEnemy = default;
        }

        public void UpdateAlarmEnemy()
        {
            AlarmUpdate();

            _targetEnemy = _isAlarm ? new Optionals<IBattleEnemy>(_alarmCharacters[0].enemy) : new();
        }

        public void AddAlarmEnemy(IBattleEnemy character, float damage = 0)
        {
            for (int i = 0; i < _alarmCharacters.Count; i++)
            {
                try
                {
                    if (_alarmCharacters[i].enemy.GetGameObject.Equals(character.GetGameObject))
                    {
                        _alarmCharacters[i] = new EnemyPriority(character, damage + _alarmCharacters[i].damage);
                        UpdateAlarmEnemy();
                        return;
                    }
                }
                catch
                {
                    _alarmCharacters.RemoveAt(i);
                    Debug.LogError("Custom exception from alarm");
                    continue;
                }
            }

            _alarmCharacters.Add(new EnemyPriority(character, damage));
            UpdateAlarmEnemy();
        }

        public bool ContainsEnemy(IBattleEnemy enemy)
        {
            foreach (EnemyPriority priority in _alarmCharacters)
                if (priority.enemy.Equals(enemy))
                    return true;

            return false;
        }

        public void AlarmReduction(float timePursuit)
        {
            for (int i = 0; i < _alarmCharacters.Count; i++)
                _alarmCharacters[i] = Reduction(_alarmCharacters[i]);

            _alarmCharacters = _alarmCharacters.Where(enemy => enemy.threatReduction < timePursuit || enemy.damage > 0).ToList();
        }

        private EnemyPriority Reduction(EnemyPriority e)
        {
            e.threatReduction += Time.deltaTime;
            return e;
        }

        private void AlarmUpdate()
        {
            ClearTarget();

            if (_alarmCharacters.Count == 0)
            {
                _isAlarm = false;
            }
            else
            {
                _isAlarm = true;
                _alarmCharacters = _alarmCharacters.OrderBy(n => n.damage).Reverse().ToList();

                #region "Не понятно для чего было"
                //List<EnemyPriority> tempList = new List<EnemyPriority>();

                //foreach (EnemyPriority priority in _alarmCharacters)
                //{
                //    if (tempList.Count == 0)
                //        tempList.Add(priority);
                //    else
                //    {
                //        int i = priority.enemy.GetGameObject.GetInstanceID();
                //        int j = tempList[tempList.Count - 1].enemy.GetGameObject.GetInstanceID();
                //        if (j > i)
                //            tempList.Add(priority);
                //        else
                //            tempList.Insert(tempList.Count - 1, priority);
                //    }
                //}

                //_alarmCharacters = new List<EnemyPriority>(tempList);
                #endregion
            }
        }

        private void ClearTarget()
        {
            if (_alarmCharacters.Count == 0)
                return;

            _alarmCharacters = _alarmCharacters.Where(enemy => {
                //Debug.Log($"{enemy.enemy.GetGameObject.name}: {enemy.enemy?.GetTransform != null}/{!enemy.enemy.IsDie}/{IsVisibleUnit(enemy)}");
                return enemy.enemy?.GetTransform != null &&
                !enemy.enemy.IsDie &&
                IsVisibleUnit(enemy);
                }).ToList();
        }

        private bool IsVisibleUnit(EnemyPriority enemy)
        {
            return enemy.damage != 0 || Vision.IsVisibleUnit(enemy.enemy, _enemy.GetEyePoint, enemy.enemy.GetTransform, _vision);
        }

    }
}