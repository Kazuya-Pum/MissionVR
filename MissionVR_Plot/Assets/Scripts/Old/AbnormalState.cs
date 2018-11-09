using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//未使用
public class BuffOrdre
{
    private IEnumerator coroutine;
    private float time;
    private float ratio;

    public BuffOrdre(IEnumerator coroutine,float time,float ratio)
    {
        this.coroutine = coroutine;
        this.time = time;
        this.ratio = ratio;
    }

    public IEnumerator CoroutineProp
    {
        get
        {
            return this.coroutine;
        }
    }

    public float TimeProp
    {
        get
        {
            return this.time;
        }
    }

    public float RatioProp
    {
        get
        {
            return this.ratio;
        }
    }
}

//未使用
public class DotOrdre
{
    private IEnumerator coroutine;
    private float variate;
    private float time;
    private float interval;

    public DotOrdre(IEnumerator coroutine, float variate, float time,float interval)
    {
        this.coroutine = coroutine;
        this.variate = variate;
        this.time = time;
        this.interval = interval;
    }

    public IEnumerator CoroutineProp
    {
        get
        {
            return this.coroutine;
        }
    }

    public float VariateProp
    {
        get
        {
            return this.variate;
        }
    }
    
    public float TimeProp
    {
        get
        {
            return this.time;
        }
    }

    public float IntervalProp
    {
        get
        {
            return this.interval;
        }
    }
}


public class Coroutine
{
    private IEnumerator coroutine;
    private int count;

    public Coroutine(IEnumerator coroutine,int count)
    {
        this.coroutine = coroutine;
        this.count = count;
    }

    public IEnumerator CoroutineProp
    {
        get
        {
            return this.coroutine;
        }
    }

    public int CountProp
    {
        get
        {
            return this.count;
        }
    }

}

public class AbnormalState:MonoBehaviour
{
    private int count=0;
    private List<Coroutine> coroutineList = new List<Coroutine>();

    public AbnormalState(string sendMessage,  float time, float ratio, int[] typeNum)
    {
        this.coroutineList.Add(new Coroutine(Buff(sendMessage, time, ratio, typeNum,count),count));
        this.count++;
        this.StartBuffOrDot();
    }

    public AbnormalState(string sendMessage, float variate, float time, float interval, int[] typeNum)
    {
        this.coroutineList.Add(new Coroutine(Dot(sendMessage, variate, time, interval, typeNum,count),count));
        this.count++;
        this.StartBuffOrDot();
    }

    public void StartBuffOrDot()
    {
        for (int i = 0; i < coroutineList.Count; i++)
        {
            this.StartCoroutine(coroutineList[i].CoroutineProp);
        }
    }

    public void StopBuffOrDot()
    {
        for (int i = 0; i < coroutineList.Count; i++)
        {
            this.StopCoroutine(coroutineList[i].CoroutineProp);
        }
    }

    public void PulsBuff(string sendMessage, float time, float ratio, int[] typeNum)
    {
        this.coroutineList.Add(new Coroutine(Buff(sendMessage, time, ratio, typeNum, this.count), this.count));
        this.StartCoroutine(coroutineList[this.count].CoroutineProp);
        this.count++;
    }

    public void PulsBuff(string sendMessage, float variate, float time, float interval, int[] typeNum)
    {
        this.coroutineList.Add(new Coroutine(Dot(sendMessage, variate, time, interval, typeNum, this.count), this.count));
        this.StartCoroutine(coroutineList[this.count].CoroutineProp);
        this.count++;
    }

    public IEnumerator Buff(string sendMessage, float time, float ratio, int[] typeNum,int num)
    {
        SendMessage(sendMessage, ratio);
        yield return new WaitForSeconds(time);
        SendMessage(sendMessage, 1 / ratio);
        if (coroutineList.Count <= 0)
        {
            SendMessage("FinshAbnormal", typeNum);
        }
        else
        {
            this.List(num);
        }
    }

    public IEnumerator Dot(string sendMessage, float variate, float time, float interval, int[] typeNum,int num)
    {
        while (time >= interval)
        {
            time = time - interval;
            SendMessage(sendMessage, variate);
            yield return new WaitForSeconds(interval);
        }
        if (coroutineList.Count<= 0)
        {
            SendMessage("FinshAbnormal", typeNum);
        }
        else
        {
            this.List(num);
        }
    }


    public void List(int num)
    {
        for(int i=0;i<coroutineList.Count;i++)
        {
            if(coroutineList[i].CountProp>=num)
            {
                coroutineList.RemoveAt(i);
                break;
            }
        }
    }
}
