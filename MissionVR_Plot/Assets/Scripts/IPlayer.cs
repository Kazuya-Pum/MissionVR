using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public interface IPlayer
    {
        void Damage(int d);//ダメージ処理

        Transform GetPlayerTransform(); //銃口のTransform取得
    }
}