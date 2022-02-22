using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GlobalEnums;
using Modding;
using Satchel;
using static Satchel.GameObjectUtils;
using UnityEngine;
using UnityEngine.UI;
using Mono.Cecil.Cil;
using MonoMod;
using TMPro;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Language;
using Vasi;
using SereCore;
using System.Windows.Documents;
using System.Web.UI.MobileControls;

namespace Abskoth
{
    public partial class Abskoth : Mod
    {
        public const string Version = "0.1.0.0";
        public override string GetVersion() => Abskoth.Version;
        internal static Abskoth Instance;
        internal bool inAbskothFight = false;
        internal int AbskothPhase = 0;

        internal GameObject markothPrefab = null;
        private const string _scene = SceneNames.Deepnest_East_16;
        private const string _obj = "Warrior/Ghost Warrior Markoth";

        public override List<(string, string)> GetPreloadNames() => new List<(string, string)> { (_scene, _obj) };

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;

            if (preloadedObjects != null)
                markothPrefab = preloadedObjects[_scene][_obj];
            GameObject.DontDestroyOnLoad(markothPrefab);

            ModHooks.OnEnableEnemyHook += EnemyEnabled;
            ModHooks.BeforeSceneLoadHook += OnSceneLoad;
            ModHooks.ObjectPoolSpawnHook += OnObjectSpawn;
            
            On.HealthManager.TakeDamage += OnTakeDamage;
            On.HutongGames.PlayMaker.Actions.RandomFloat.OnEnter += OnRandomFloat;
        }

        private GameObject OnObjectSpawn(GameObject arg)
        {
            String name = arg.name;
            if (name.Contains("("))
                name = name.Substring(0, arg.name.IndexOf("(")).Trim();
            if (!name.Equals("Shot Markoth Nail"))
                return arg;

            if (inAbskothFight)
            {
                if (!GlobalSaveData.withNail)
                {
                    GameObject.Destroy(arg);
                    return null;
                }

                PlayMakerFSM fsm = arg.LocateMyFSM("Control");
                foreach (FsmFloat f in fsm.FsmVariables.FloatVariables)
                    f.Value = GetFValue(f);
            }

            return arg;
        }

        private void OnRandomFloat(On.HutongGames.PlayMaker.Actions.RandomFloat.orig_OnEnter orig, HutongGames.PlayMaker.Actions.RandomFloat self)
        {
            String name = self.Fsm.GameObject.name;
            Vector3 pos = HeroController.instance.transform.position;

            if (name.Contains("("))
                name = name.Substring(0, self.Fsm.GameObject.name.IndexOf("(")).Trim();
            if (name.Equals("Shot Markoth Nail") && self.Fsm.Name.Equals("Control"))
            {
                if (self.max.Value == 43f || self.max.Value == 72.5f || self.max.Value == 80.0f || self.max.Value == 75.0f)
                    self.min.Value = AbskothPhase == 0 ? 48.5F : AbskothPhase == 1 ? 40.0F : 51.0F;
                else self.min.Value = pos.y < 34.0F ? 19.0F : pos.y < 58.0F ? 33.0F : pos.y < 152.0F ? 54.0F : 152.0F;
            }

            orig(self);
        }

        private float GetFValue(FsmFloat f)
        {
            if (f.Name.Equals("X Max")) // 48.5 72.5     40.0 80.0     51.0 75.0
                f.Value = AbskothPhase == 0 ? 72.5F : AbskothPhase == 1 ? 80.0F : 75.0F;
            if (f.Name.Equals("Y Max")) // 19.0 32.0     33.0 56.0     152.0 163.5               150.0 for climb
                f.Value = AbskothPhase == 0 ? 32.0F : AbskothPhase == 1 ? 56.0F : HeroController.instance.transform.position.y >= 152.0F ? 150.0F : 163.5F;

            return f.Value;
        }



        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            String name = self.gameObject.name;
            if (name.Contains("("))
                name = name.Substring(0, self.gameObject.name.IndexOf("(")).Trim();

            if (inAbskothFight && name.Equals("Ghost Warrior Markoth"))
                hitInstance.DamageDealt = 0;

            orig(self, hitInstance);
        }

        private string OnSceneLoad(string arg)
        {
            inAbskothFight = false;
            return arg;
        }

        public bool EnemyEnabled(GameObject enemy, bool isDead)
        {
            if (isDead)
                return isDead;

            if (enemy.name.Equals("Absolute Radiance"))
            {
                inAbskothFight = true;
                AbskothPhase = 0;
                if (markothPrefab != null)
                    GameManager.instance.StartCoroutine(SpawnMarkoth(enemy));
                else Log("No Markoth Prefab to spawn.");
            }

            return false;
        }

        IEnumerator SpawnMarkoth(GameObject enemy)
        {
            yield return new WaitForSeconds(1.6f);

            Log("Spawning " + Abskoth.GlobalSaveData.numMarkoths + " Markoths!");
            for (int i = 0; i < Abskoth.GlobalSaveData.numMarkoths; i++)
            {
                Log("Markoth: " + markothPrefab);
                GameObject go = GameObject.Instantiate(markothPrefab);
                go.SetActive(true);

                switch (i) {
                    case 0: go.transform.position = new Vector3(52, HeroController.instance.transform.position.y + 5, -1); break;
                    case 1: go.transform.position = new Vector3(68.7F, HeroController.instance.transform.position.y + 5, -1); break;
                    case 2: go.transform.position = new Vector3(44, HeroController.instance.transform.position.y + 5, -1); break;
                    case 3: go.transform.position = new Vector3(76.7F, HeroController.instance.transform.position.y + 5, -1); break;
                }

                go.AddComponent<AbskothBehaviour>();

                foreach (FsmBool b in go.LocateMyFSM("Shield Attack").FsmVariables.BoolVariables)
                {
                    if (b.Name.Equals("Rage"))
                        b.Value = true;
                }
                foreach (FsmBool b in go.LocateMyFSM("Attacking").FsmVariables.BoolVariables)
                {
                    if (b.Name.Equals("Rage"))
                        b.Value = true;
                }
            }
        }
    }
    public class AbskothBehaviour : MonoBehaviour
    {
        private int phase = 0;
        private bool update = false;
        private GameObject absrad;
        private HealthManager hm;
        private PlayMakerFSM fsm;
        private float[] phaseUp = { 0, 25, 133 };
        private float[] phaseDown = { 0, 17, 133 };
        private float[] phaseLeft = { 0, -6, 8 };
        private float[] phaseRight = { 0, 4, 2 };

        public void Awake()
        {
            phase = 0;
            update = false;
            absrad = GameObject.Find("Absolute Radiance");
            hm = absrad.GetComponent<HealthManager>();
            fsm = absrad.LocateMyFSM("Control");

            foreach (FsmVector3 v in this.gameObject.LocateMyFSM("Movement").FsmVariables.Vector3Variables)
            {
                switch (v.Name)
                {
                    case "P1": v.Value = new Vector2(60.00F, 28.00F); break;
                    case "P2": v.Value = new Vector2(60.00F, 23.00F); break;
                    case "P3": v.Value = new Vector2(48.50F, 21.00F); break;
                    case "P4": v.Value = new Vector2(72.50F, 21.00F); break;
                    case "P5": v.Value = new Vector2(72.10F, 27.50F); break;
                    case "P6": v.Value = new Vector2(48.80F, 27.50F); break;
                    case "P7": v.Value = new Vector2(55.50F, 25.00F); break;
                    case "P8": v.Value = new Vector2(64.50F, 25.00F); break;
                }
            }
        }

        public void FixedUpdate()
        {
            CheckAbsradHealth();
            if (update)
                UpdatePosition();
        }

        private void CheckAbsradHealth()
        {
            int hp = hm.hp;
            if (hp > 1850)
                return;
            if (hp > 1100 && phase == 1)
                return;
            if (fsm.ActiveStateName.Equals("Knight Break Antic"))
                GameObject.Destroy(this.gameObject);
            if (hp <= 1100 && phase == 2)
                return;
            phase += 1;
            Abskoth.Instance.AbskothPhase = phase;
            update = true;
        }

        private void UpdatePosition()
        {
            update = false;
            foreach (FsmVector3 v in this.gameObject.LocateMyFSM("Movement").FsmVariables.Vector3Variables)
            {
                switch (v.Name)
                {
                    case "P1": v.Value = new Vector2(60.00F, 28.00F + phaseUp[phase]); break;
                    case "P2": v.Value = new Vector2(60.00F, 23.00F + phaseDown[phase]); break;
                    case "P3": v.Value = new Vector2(48.50F + phaseLeft[phase], 21.00F + phaseDown[phase]); break;
                    case "P4": v.Value = new Vector2(72.50F + phaseRight[phase], 21.00F + phaseDown[phase]); break;
                    case "P5": v.Value = new Vector2(72.10F + phaseRight[phase], 27.50F + phaseUp[phase]); break;
                    case "P6": v.Value = new Vector2(48.80F + phaseLeft[phase], 27.50F + phaseUp[phase]); break;
                    case "P7": v.Value = new Vector2(55.50F + (phaseLeft[phase]/2), 25.00F + phaseUp[phase]); break;
                    case "P8": v.Value = new Vector2(64.50F + (phaseRight[phase]/2), 25.00F + phaseUp[phase]); break;
                }
            }
        }
    }
}