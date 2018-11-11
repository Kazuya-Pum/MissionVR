using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConflictPattern
{
    ADDITION=0,
    PRODUCT,
    OVER_WRITE,
    FIRST_COME,
    NULL
}

namespace Abnormal
{



    public class PatternAndName
    {
        private string abnormalName;
        private ConflictPattern conflictPattern;

        public PatternAndName(string abnormalName, ConflictPattern conflictPattern)
        {
            this.abnormalName = abnormalName;
            this.conflictPattern = conflictPattern;
        }


        public string AbnormalNameProp
        {
            get
            {
                return this.abnormalName;
            }
        }

        public ConflictPattern ConflictPatternProp
        {
            get
            {
                return this.conflictPattern;
            }
        }
    }

    public class Type
    {
        private string abnormalName;
        private bool isAbnormal;
        private ConflictPattern conflictPattern;
        private AbnormalState AbnormalState;

        public Type(string abnormalName)
        {
            this.abnormalName = abnormalName;
            this.isAbnormal = false;
            this.conflictPattern = ConflictPattern.NULL;
        }

        public Type(string abnormalName, ConflictPattern pattern)
        {
            this.abnormalName = abnormalName;
            this.isAbnormal = false;
            this.conflictPattern = pattern;
        }

        public Type(PatternAndName pAndN)
        {
            this.abnormalName = pAndN.AbnormalNameProp;
            this.isAbnormal = false;
            this.conflictPattern = pAndN.ConflictPatternProp;
        }


        public string AbnormalNameProp
        {
            get
            {
                return this.abnormalName;
            }
        }

        public bool IsAbnormalProp
        {
            get
            {
                return this.isAbnormal;
            }

            set
            {
                this.isAbnormal = value;
            }
        }

        public ConflictPattern ConflictPatternProp
        {
            get
            {
                return this.conflictPattern;
            }
        }

        public AbnormalState AbnormalStateProp
        {
            get
            {
                return this.AbnormalState;
            }

            set
            {
                this.AbnormalState = value;
            }
        }

        internal static Type GetType(string v1, bool v2)
        {
            throw new NotImplementedException();
        }
    }

    public class AbnormalType : MonoBehaviour
    {
        #region 使い方
        /*
         * ※必要なもの
         * 宣言時の状態異常の種類(細かくはできる処理の下)
         * 
         * ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
         * ※できる処理(関数)
         * AbnormalOccurrence()
         * 状態異常の発生
         * 引数
         * 1.
         * int[],string typeNum　状態異常の種類intの長さ2の二次元配列で場所指定もしくはstringで名前指定
         * string sendMessage　送りたい関数名もしくはプロパティ名
         * float time　発生時間
         * float ratio　割合(等倍→1.0f)
         * 
         * 処理内容
         * 実行時にratioをsendMessageに送る
         * 発生時間経過後ratioの逆数をsendMessageに送る
         * (バフ、デバフ系の処理)
         * 
         * 2.
         * int[],string typeNum　状態異常の種類intの長さ2の二次元配列で場所指定もしくはstringで名前指定
         * string sendMessage　送りたい関数名もしくはプロパティ名
         * float variate　変化量(ダメージ量)
         * float time　発生時間
         * float interval　変化量の間隔
         * 処理内容
         * timeの間,intervalごとにvariateをsendMessageに送る
         * (毒系の処理)
         * 
         * StartAbnormals()
         * StopAbnormals()
         * かかっている状態以上の一次停止＆再開
         * 
         * OnAbnormals()
         * OffAbnormals()
         * どの状態異常にかかっているか否か
         * 今回は未使用　中に処理を好きに書いてください
         * 
         * ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
         * ※細かいとこ
         * 6種類いずれかのコンストラクタを使用します
         * 名前だけのもの(ConflictPatternはNULLになる)
         * private AbnormalType()
         * private AbnormalType(string[][] type)
         * 
         * 名前と複合するか否か
         * private AbnormalType()
         * private AbnormalType(string[][] type, ConflictPattern[][] conflictPattern)
         * 
         * 名前と複合するか否かをまとめて
         * private AbnormalType()
         * private AbnormalType(PatternAndName[][] pAndN)
         * 
         * このクラス内の変数に直接状態異常の種類を書くか宣言時に引数として与えてください
         * 引数がないものがクラス内に直接書くものです
         * 現状では引数無しの場合,名前だけのものを使用します
         * 
         * 各変数の説明
         * string[][] type
         * 状態異常の種類(名前)
         * ジャグ配列になっています
         * 一つ目のくくりはかぶっても大丈夫なもので
         * 二つ目のくくりは左のものが優先されるグループになってます
         * 例:private string[][] names = new string[一つ目][二つ目]
            {
                各配列ごとは同時に起こる
                new string[]
                {
                    "a"
                },
                new string[]
                {この中の種類はかぶらないかつ左優先
                    "b","c","d","e","f"
                },
                new string[]
                {
                    "g","h"
                },
            };

            ConflictPattern[][] conflictPattern
            同じ場所にある状態異常が複数かかるか否か
            enumは数種類ありますが現状NULLか否かでのみの判断
            private ConflictPattern[][] pattern = new ConflictPattern[][]
            {
                new ConflictPattern[]
                {
                    ConflictPattern.ADDITION,上のaにかんする状態異常が複数かかるものか
                },
                new ConflictPattern[]
                {
                    ConflictPattern.NULL,
                    ConflictPattern.NULL,
                    ConflictPattern.NULL,
                    ConflictPattern.NULL,
                    ConflictPattern.NULL
                },
                new ConflictPattern[]
                {
                    ConflictPattern.NULL,
                    ConflictPattern.NULL
                },
            };


            PatternAndName[][] pAndN
            string[][] typeとConflictPattern[][] conflictPatternを合わせたもの
             */
        #endregion

        private Type[][] type;
        private string[] names1 = new string[]
        {
            "mobeBuff","attackBuff","defenseBuff","rerodeBuff","dotHeal","Heal","dotDamege","MapDark"
        };
        private string[][] names = new string[][]
        {
        new string[]
        {
            "moveBuff"
        },
        new string[]
        {
            "","","","",""
        },
        new string[]
        {
            "",""
        },
        };

        public AbnormalType()
        {
            for (int i = 0; i < this.names.Length; i++)
            {
                for (int j = 0; j < this.names[i].Length; j++)
                {
                    this.type[i][j] = new Type(this.names[i][j]);
                }
            }
        }

        /*
        private ConflictPattern[][] pattern = new ConflictPattern[][]
        {
            new ConflictPattern[]
            {
                ConflictPattern.ADDITION,
                ConflictPattern.ADDITION,
            },
            new ConflictPattern[]
            {
                ConflictPattern.ADDITION,
                ConflictPattern.ADDITION,
                ConflictPattern.ADDITION,
            },
            new ConflictPattern[]
            {
                ConflictPattern.ADDITION,
            },
        };

        private AbnormalType()
        {
            for (int i = 0; i < this.names.Length; i++)
            {
                for (int j = 0; j < this.names[i].Length; j++)
                {
                    this.type[i][j] = new Type(this.names[i][j],pattern[i][j]);
                }
            }
        }

        */

        /*
        private PatternAndName[][] pAndN = new PatternAndName[][]
        {
            new PatternAndName[]
            {
                new PatternAndName("name1-1",ConflictPattern.ADDITION),
                new PatternAndName("name1-2",ConflictPattern.ADDITION),
            },
            new PatternAndName[]
            {
                new PatternAndName("name2-1",ConflictPattern.FIRST_COME),
                new PatternAndName("name2-2",ConflictPattern.OVER_WRITE),
                new PatternAndName("name2-3",ConflictPattern.PRODUCT),
            },
            new PatternAndName[]
            {
                new PatternAndName("name3-1",ConflictPattern.ADDITION),
            },
        };

        private AbnormalType()
        {
            for (int i = 0; i < this.names.Length; i++)
            {
                for (int j = 0; j < this.names[i].Length; j++)
                {
                    this.type[i][j] = new Type(this.pAndN[i][j]);
                }
            }
        }

             */

        private AbnormalType(string[][] type)
        {
            for (int i = 0; i < type.Length; i++)
            {
                for (int j = 0; j < type[i].Length; j++)
                {
                    this.type[i][j] = new Type(type[i][j]);
                }
            }
        }

        private AbnormalType(string[][] type, ConflictPattern[][] conflictPattern)
        {
            for (int i = 0; i < type.Length; i++)
            {
                for (int j = 0; j < type[i].Length; j++)
                {
                    this.type[i][j] = new Type(type[i][j], conflictPattern[i][j]);
                }
            }
        }

        private AbnormalType(PatternAndName[][] pAndN)
        {
            for (int i = 0; i < type.Length; i++)
            {
                for (int j = 0; j < type[i].Length; j++)
                {
                    this.type[i][j] = new Type(pAndN[i][j]);
                }
            }
        }

        public void AbnormalOccurrence(int[] typeNum, string sendMessage, float time, float ratio)
        {
            for (int j = 0; j < typeNum[1]; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    return;
                }
            }
            for (int j = typeNum[1]; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    FinishAbnormal(new int[] { typeNum[0], j });
                    StopAbnormal(new int[] { typeNum[0], j });
                    break;
                }
            }
            if (this.type[typeNum[0]][typeNum[1]].IsAbnormalProp)
            {
                UpdateAbnormal(sendMessage, time, ratio, typeNum);
                return;
            }
            this.type[typeNum[0]][typeNum[1]].IsAbnormalProp = true;
            this.type[typeNum[0]][typeNum[1]].AbnormalStateProp = new AbnormalState(sendMessage, time, ratio, typeNum);
        }

        public void AbnormalOccurrence(string typeName, string sendMessage, float time, float ratio)
        {
            int[] typeNum = new int[2];
            int i = 0;
            int j = 0;
            for (; i < this.type.Length || typeName == type[i][j].AbnormalNameProp; i++)
            {
                for (; j < this.type[i].Length || typeName == type[i][j].AbnormalNameProp; j++)
                {
                    typeNum[1] = j;
                }
                typeNum[0] = i;
            }

            for (j = 0; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    return;
                }
            }

            for (j = typeNum[1]; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    FinishAbnormal(new int[] { typeNum[0], j });
                    StopAbnormal(new int[] { typeNum[0], j });
                    break;
                }
            }
            if (this.type[typeNum[0]][typeNum[1]].IsAbnormalProp)
            {
                UpdateAbnormal(sendMessage, time, ratio, typeNum);
                return;
            }
            this.type[typeNum[0]][typeNum[1]].IsAbnormalProp = true;
            this.type[typeNum[0]][typeNum[1]].AbnormalStateProp = new AbnormalState(sendMessage, time, ratio, typeNum);

        }

        private void AbnormalOccurrence(int[] typeNum, string sendMessage, float variate, float time, float interval)
        {
            for (int j = 0; j < typeNum[1]; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    return;
                }
            }

            for (int j = typeNum[1]; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    FinishAbnormal(new int[] { typeNum[0], j });
                    StopAbnormal(new int[] { typeNum[0], j });
                    break;
                }
            }
            if (this.type[typeNum[0]][typeNum[1]].IsAbnormalProp)
            {
                UpdateAbnormal(sendMessage, variate, time, interval, typeNum);
                return;
            }
            this.type[typeNum[0]][typeNum[1]].IsAbnormalProp = true;
            this.type[typeNum[0]][typeNum[1]].AbnormalStateProp = new AbnormalState(sendMessage, variate, time, interval, typeNum);
        }

        private void AbnormalOccurrence(string typeName, string sendMessage, float variate, float time, float interval)
        {
            int[] typeNum = new int[2];
            int i = 0;
            int j = 0;
            for (; i < this.type.Length || typeName == type[i][j].AbnormalNameProp; i++)
            {
                for (; j < this.type[i].Length || typeName == type[i][j].AbnormalNameProp; j++)
                {
                    typeNum[1] = j;
                }
                typeNum[0] = i;
            }

            for (j = 0; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    return;
                }
            }

            for (j = typeNum[1]; j < this.type[typeNum[0]].Length; j++)
            {
                if (this.type[typeNum[0]][j].IsAbnormalProp)
                {
                    FinishAbnormal(new int[] { typeNum[0], j });
                    StopAbnormal(new int[] { typeNum[0], j });
                    break;
                }
            }
            if (this.type[typeNum[0]][typeNum[1]].IsAbnormalProp)
            {
                UpdateAbnormal(sendMessage, variate, time, interval, typeNum);
                return;
            }
            this.type[typeNum[0]][typeNum[1]].IsAbnormalProp = true;
            this.type[typeNum[0]][typeNum[1]].AbnormalStateProp = new AbnormalState(sendMessage, variate, time, interval, typeNum);

        }

        private void FinishAbnormal(int[] typeNum)
        {
            type[typeNum[0]][typeNum[1]].IsAbnormalProp = false;
        }

        private void StartAbnormals()
        {
            for (int i = 0; i < this.type.Length; i++)
            {
                for (int j = 0; j < this.type[i].Length; j++)
                {
                    if (this.type[i][j].IsAbnormalProp)
                    {
                        type[i][j].AbnormalStateProp.StartBuffOrDot();
                    }
                }
            }
        }

        private void StopAbnormals()
        {
            for (int i = 0; i < this.type.Length; i++)
            {
                for (int j = 0; j < this.type[i].Length; j++)
                {
                    if (this.type[i][j].IsAbnormalProp)
                    {
                        type[i][j].AbnormalStateProp.StopBuffOrDot();
                    }
                }
            }
        }

        private void StopAbnormal(int[] typeNum)
        {
            type[typeNum[0]][typeNum[1]].AbnormalStateProp.StopBuffOrDot();
        }

        /*
        private void OnAbnormals()
        {
            for (int i = 0; i < this.type.Length; i++)
            {
                for (int j = 0; j < this.type[i].Length; j++)
                {
                    if (this.type[i][j].IsAbnormalProp)
                    {
                        かかっている状態異常に何かするかの処理
                    }
                }
            }
        }
        */

        /*
        private void OffAbnormals()
        {
            for (int i = 0; i < this.type.Length; i++)
            {
                for (int j = 0; j < this.type[i].Length; j++)
                {
                    if (!this.type[i][j].IsAbnormalProp)
                    {
                        かかっていない状態異常に何かするかの処理
                    }
                }
            }
        }
        */


        private void UpdateAbnormal(string sendMessage, float variate, float time, float interval, int[] typeNum)
        {
            /*
            switch(type[typeNum[0]][typeNum[1]].ConflictPatternProp)
            {
                case ConflictPattern.ADDITION:
                    break;
                case ConflictPattern.PRODUCT:
                    break;
                case ConflictPattern.OVER_WRITE:
                    break;
                case ConflictPattern.FIRST_COME:
                    break;
                case ConflictPattern.NULL:
                    break;
                default:
                    break;
            }*/
            if (type[typeNum[0]][typeNum[1]].ConflictPatternProp != ConflictPattern.NULL)
            {
                type[typeNum[0]][typeNum[1]].AbnormalStateProp.PulsBuff(sendMessage, variate, time, interval, typeNum);
            }
        }

        private void UpdateAbnormal(string sendMessage, float time, float ratio, int[] typeNum)
        {
            /*
            switch(type[typeNum[0]][typeNum[1]].ConflictPatternProp)
            {
                case ConflictPattern.ADDITION:
                    break;
                case ConflictPattern.PRODUCT:
                    break;
                case ConflictPattern.OVER_WRITE:
                    break;
                case ConflictPattern.FIRST_COME:
                    break;
                case ConflictPattern.NULL:
                    break;
                default:
                    break;
            }*/
            if (type[typeNum[0]][typeNum[1]].ConflictPatternProp != ConflictPattern.NULL)
            {
                type[typeNum[0]][typeNum[1]].AbnormalStateProp.PulsBuff(sendMessage, time, ratio, typeNum);
            }
        }

        /*
        private void UpdateAbnormals()
        {
            for (int i = 0; i < this.type.Length; i++)
            {
                for (int j = 0; j < this.type[i].Length; j++)
                {
                    if (this.type[i][j].IsAbnormalProp)
                    {

                    }
                }
            }
        }
        */
    }
}