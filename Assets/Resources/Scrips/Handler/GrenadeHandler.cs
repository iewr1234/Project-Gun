using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EPOOutline;

public class GrenadeHandler : MonoBehaviour
{
    [Header("---Access Script---")]
    private CharacterController charCtr;

    [Header("---Access Component---")]
    public LineRenderer lineRdr;
    public MeshRenderer rangeMr;
    private Transform grenadesTf;
    private List<GameObject> grenades = new List<GameObject>();
    private Transform FX_Tf;
    private List<ParticleSystem> FX_List = new List<ParticleSystem>();

    [Header("--- Assignment Variable---")]
    public GameObject curGrenade;
    public ParticleSystem curFX;
    public float throwRange;
    public float blastRange;
    public int damage;
    [Space(5f)]

    public float moveSpeed;
    public float rotSpeed;

    private Vector3[] points;
    private int index;
    private bool isThrow;

    public void SetComponents(CharacterController _charCtr)
    {
        charCtr = _charCtr;

        lineRdr = GetComponent<LineRenderer>();
        lineRdr.positionCount = 60;
        lineRdr.startWidth = 0.03f;
        lineRdr.enabled = false;
        rangeMr = transform.Find("ExplosionRange").GetComponent<MeshRenderer>();
        rangeMr.gameObject.SetActive(false);

        grenadesTf = transform.Find("Grenades");
        for (int i = 0; i < grenadesTf.childCount; i++)
        {
            var grenade = grenadesTf.GetChild(i).gameObject;
            grenades.Add(grenade);
            grenade.SetActive(false);
        }
        FX_Tf = transform.Find("FX");
        FX_List = FX_Tf.GetComponentsInChildren<ParticleSystem>().ToList();
    }

    private void Update()
    {
        if (!isThrow) return;

        var nextPoint = points[index];
        curGrenade.transform.position = Vector3.MoveTowards(curGrenade.transform.position, nextPoint, moveSpeed * Time.deltaTime);
        curGrenade.transform.Rotate(transform.forward, rotSpeed);
        if (curGrenade.transform.position == nextPoint)
        {
            index++;
            if (index == points.Length)
            {
                curFX.transform.SetParent(charCtr.transform.parent);
                curFX.transform.position = curGrenade.transform.position;
                curFX.Play();
                OnHitTargets();
                StartCoroutine(Coroutine_ResetGrenadeFX(curFX.main.duration));

                curGrenade.transform.SetParent(grenadesTf, false);
                curGrenade.SetActive(false);
                isThrow = false;
            }
        }

        void OnHitTargets()
        {
            for (int i = 0; i < charCtr.throwInfo.targetList.Count; i++)
            {
                var target = charCtr.throwInfo.targetList[i];
                var dir = Vector3.Normalize(target.transform.position - curGrenade.transform.position);
                target.OnHit(dir, damage);
            }
        }
    }

    public void SetGrenadeInfo(GrenadeDataInfo grenadeData)
    {
        curGrenade = grenades.Find(x => x.name == grenadeData.grenadeName);
        curFX = FX_List.Find(x => x.name == grenadeData.FX_name);
        throwRange = grenadeData.throwRange;
        blastRange = grenadeData.blastRange * 0.1f;
        damage = grenadeData.damage;

        rangeMr.transform.localScale = new Vector3(blastRange, blastRange, blastRange);
    }

    public void ThrowGrenade(Vector3 targetPos)
    {
        curGrenade.transform.SetParent(charCtr.transform.parent);
        curGrenade.transform.LookAt(targetPos);
        points = DataUtility.GetParabolaPoints(lineRdr.positionCount, curGrenade.transform.position, targetPos);
        index = 0;
        isThrow = true;
    }

    public IEnumerator Coroutine_ResetGrenadeFX(float time)
    {
        yield return new WaitForSecondsRealtime(time + 1f);

        curFX.transform.SetParent(FX_Tf, false);
    }
}
