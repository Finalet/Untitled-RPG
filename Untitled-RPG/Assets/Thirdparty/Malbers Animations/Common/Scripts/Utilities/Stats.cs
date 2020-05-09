using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System;

namespace MalbersAnimations
{
    public class Stats : MonoBehaviour , IAnimatorListener
    {
        /// <summary>List of Stats</summary>
        public List<Stat> stats = new List<Stat>();
        /// <summary>List of Stats Converted to Dictionary</summary>
        public Dictionary<int, Stat> stats_D;

        /// <summary>Stored Stat to use the 'Pin' Methods</summary>
        public Stat PinnedStat;

        private void Awake() { stats_D = new Dictionary<int, Stat>(); }
       
        private void OnDisable() { StopAllCoroutines(); }

        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);
        }

        private void Start()
        {
            StopAllCoroutines();

            foreach (var stat in stats)
            {
                stat.InitializeStat(this);
                stats_D.Add(stat.ID, stat); //Convert them to Dictionary
            }
        }

        /// <summary>Updates all Stats</summary>
        public virtual void Stats_Update()
        {
            foreach (var s in stats)
                s.UpdateStat();
        }

        public virtual void Stats_Update(StatID iD)
        {
            Stats_Update(iD.ID);
        }

        public virtual void Stats_Update(int iD)
        {
            Stat_Get(iD);
            PinnedStat?.UpdateStat();
        }

        public virtual void Stat_Enable(StatID iD)
        {
            Stat_Get(iD);
            PinnedStat?.SetActive(true);
        }

        public virtual void Stat_Disable(StatID iD)
        {
            Stat_Get(iD);
            PinnedStat?.SetActive(false);
        }

       // public virtual void _DisableStat(StatID iD)    { Stat_Disable(iD); } //OLD VERSION
    

        public virtual void DegenerateOff(StatID ID)
        {
            Stat_Get(ID);
            PinnedStat?.SetDegeneration(false);
        }

        public virtual void DegenerateOn(StatID ID)
        {
            Stat_Get(ID);
            PinnedStat?.SetDegeneration(true);
        }

        /// <summary>Set PinnedStat searching for a Stat Name</summary>
        public virtual void Stat_Pin(string name) { Stat_Get(name); }

        /// <summary>Set PinnedStat searching for a int ID value</summary>
        public virtual void Stat_Pin(int ID) { Stat_Get(ID); }

        /// <summary>Set PinnedStat searching for a StatID</summary>
        public virtual void Stat_Pin(StatID ID) { Stat_Get(ID); }
      

        /// <summary>Find a Stat Using its name for the ID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(string name)
        {
            PinnedStat = stats.Find(item => item.Name == name);
            return PinnedStat;
        }

        /// <summary>Find a Stat Using a int Value for the ID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(int ID)
        {
            PinnedStat = null;
            if (stats_D.TryGetValue(ID, out PinnedStat))
            {
                return PinnedStat;
            }
            return PinnedStat;
        }
        /// <summary>Find a Stat Using an IntVar and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(IntVar ID) { return Stat_Get(ID.Value); }

        /// <summary>Find a Stat Using a StatID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(StatID ID) { return Stat_Get(ID.ID); }

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(float value)  { PinnedStat?.Modify(value); }

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(FloatVar value) { PinnedStat?.Modify(value.Value); }

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_SetMult(float value) { PinnedStat?.SetMultiplier(value); }

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_SetMult(FloatVar value) { PinnedStat?.SetMultiplier(value.Value); }

        /// <summary>Modify Stat Value in a X time period(Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(float value, float time) { PinnedStat?.Modify(value, time); }

        /// <summary>Modify Stat Value in 1 second period(Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue_1Sec(float value) { PinnedStat?.Modify(value, 1); }

        /// <summary>Set  Stat Value to a fixed Value</summary>
        public virtual void Stat_Pin_SetValue(float value) { PinnedStat.SetValue(value); }

        /// <summary>Modify the Pinned Stat MAX Value (Add or remove to the Max Value) </summary>
        public virtual void Stat_Pin_ModifyMaxValue(float value) { PinnedStat?.ModifyMAX(value); }

        /// <summary>Set the Pinned Stat MAX Value </summary>
        public virtual void Stat_Pin_SetMaxValue(float value) { PinnedStat?.SetMAX(value); }

        /// <summary> Enable/Disable the Pinned Stat Regeneration Rate </summary>
        public virtual void Stat_Pin_Modify_RegenRate(float value) { PinnedStat?.ModifyRegenRate(value); }

       // public virtual void _PinStat(StatID ID) { Stat_Get(ID); }

        /// <summary> Enable/Disable the Pinned Stat Degeneration </summary>
       // public virtual void _PinStatDegenerate(bool value) { Stat_Pin_Degenerate(value); }
        
        /// <summary> Enable/Disable the Pinned Stat Degeneration </summary>
        public virtual void Stat_Pin_Degenerate(bool value) { PinnedStat?.SetDegeneration(value); }

        /// <summary>Enable/Disable the Pinned Stat Regeneration </summary>
        public virtual void Stat_Pin_Regenerate(bool value) { PinnedStat?.SetRegeneration(value); }
      //  public virtual void _PinStatRegenerate(bool value) { Stat_Pin_Regenerate(value); }

        /// <summary> Enable/Disable the Pinned Stat</summary>
        public virtual void Stat_Pin_Enable(bool value) { PinnedStat?.SetActive(value); }

        /// <summary>Modify the Pinned Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds</summary>
        public virtual void Stat_Pin_ModifyValue(float newValue, int ticks, float timeBetweenTicks) { PinnedStat?.Modify(newValue, ticks, timeBetweenTicks); }

        /// <summary> Clean the Pinned Stat from All Regeneration/Degeneration and Modify Tick Values </summary>
        public virtual void Stat_Pin_CleanCoroutines() { PinnedStat?.CleanRoutines(); }
    }


    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///STAT CLASS
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class Stat
    {
        #region Variables 
        /// <summary> Is the Stat Active? </summary>
        [SerializeField] private bool active = true;
        /// <summary> ID of the Stat</summary>
        public StatID ID;
        /// <summary> Value and Default/RestoreValue of the Stat</summary>
        [SerializeField] private FloatReference value = new FloatReference(0);
        /// <summary> Restore Value of the Stat  </summary>
        [SerializeField] private FloatReference maxValue = new FloatReference(100);
        /// <summary>Min Value of the Stat</summary>
        [SerializeField] private FloatReference minValue = new FloatReference(0);

        /// <summary>Multiplier to modify the Stat value</summary>
        [SerializeField] private FloatReference multiplier = new FloatReference(1);

        /// <summary>Can the Stat regenerate overtime</summary>
        [SerializeField] private bool regenerate = false;
        /// <summary>Regeneration Rate. Change the Speed of the Regeneration</summary>
        public FloatReference RegenRate;
        /// <summary>Regeneration Rate. When the value is modified this will increase or decrease it over time.</summary>
        public FloatReference RegenWaitTime = new FloatReference(0);
        /// <summary>Regeneration Rate. When the value is modified this will increase or decrease it over time.</summary>
        public FloatReference DegenWaitTime = new FloatReference(0);
        /// <summary>Can the Stat degenerate overtime</summary>
        [SerializeField] private bool degenerate = false;
        /// <summary>Degeneration Rate. Change the Speed of the Degeneration</summary>
        public FloatReference DegenRate;
        /// <summary>If greater than zero, the Stat cannot be modify until the inmune time have passed</summary>
        public FloatReference InmuneTime;
        /// <summary>If the ResetStat funtion is called it will reset to Max or Low Value</summary>
        public ResetTo resetTo = ResetTo.MaxValue;
        /// <summary> Save the Last State of the Regeneration bool</summary>
        private bool regenerate_LastValue;
        /// <summary> Save the Last State of the Regeneration bool</summary>
        private bool degenerate_LastValue;
        /// <summary> Is the Stat Below the Below Value</summary>
        private bool isBelow = false;
        /// <summary> Is the Stat Above the Above Value</summary>
        private bool isAbove = false;
        #endregion

        #region Events
        public bool ShowEvents = false;
        public UnityEvent OnStatFull = new UnityEvent();
        public UnityEvent OnStatEmpty = new UnityEvent();
        public float Below;
        public float Above;
        public UnityEvent OnStatBelow = new UnityEvent();
        public UnityEvent OnStatAbove = new UnityEvent();
        public FloatEvent OnValueChangeNormalized = new FloatEvent();
        public FloatEvent OnValueChange = new FloatEvent();
        public BoolEvent OnDegenereate = new BoolEvent();
        #endregion

        #region Properties
        /// <summary>Is the Stat Enabled? when Disable no modification can be done. All current modification can't be stopped</summary>
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                if (value)
                    StartRegeneration(); //If the Stat was activated start the regeneration
                else
                    StopRegeneration();
            }
        }

        public string Name
        {
            get
            {
                if (ID != null)
                {
                    return ID.name;
                }
                return string.Empty;
            }
        }

        /// <summary> Current value of the Stat</summary>
        public float Value
        {
            get { return value; }
            set
            {
                if (!Active) return;                //If the  Stat is not Active do nothing

                if (this.value.Value != value)      //Check the code below only if the value has changed
                    SetValue(value);
            }
        }

        /// <summary> Current Multiplier of the Stat</summary>
        public float Multiplier => multiplier.Value;

        /// <summary>Returns the Normalized value of the Stat</summary>
        public float NormalizedValue => Value / MaxValue;

        /// <summary>If True: The Stat cannot be modify </summary>
        public bool IsInmune { get; set; }      

        /// <summary>Maximum Value of the Stat</summary>
        public float MaxValue { get => maxValue.Value; set => maxValue.Value = value; }

        /// <summary>Minimun Value of the Stat </summary>
        public float MinValue { get => minValue.Value; set => minValue.Value = value; }

        /// <summary>Is the Stat Regenerating?</summary>
        public bool IsRegenerating { get; private set; }

        /// <summary>Is the Stat Degenerating?</summary>
        public bool IsDegenerating { get; private set; }
        
        /// <summary>Can the Stat Regenerate over time</summary>
        public bool Regenerate
        {
            get { return regenerate; }
            set
            {
                regenerate = value;
                regenerate_LastValue = regenerate;           //In case Regenerate is changed 
                
                
                StartRegeneration();
            }
        }

        /// <summary> Can the Stat Degenerate over time </summary>
        public bool Degenerate
        {
            get { return degenerate; }
            set
            {
                degenerate = value;
                degenerate_LastValue = degenerate;           //In case Regenerate is changed 


                OnDegenereate.Invoke(value);

                if (degenerate)
                {
                    regenerate = false;     //Do not Regenerate if we are Degenerating
                    StartDegeneration();
                    StopRegeneration();
                }
                else
                {
                    regenerate = regenerate_LastValue;   //If we are no longer Degenerating Start Regenerating again in case the Regenerate was true
                    StopDegeneration();
                    StartRegeneration();
                }
            }
        }

        #endregion

        private WaitForSeconds InmuneWait;

        internal void InitializeStat(MonoBehaviour holder)
        {
            isAbove = isBelow = false;
            Coroutine = holder;                             //Save the Monobehaviour to save coroutines

            if (value.Value > Above) isAbove = true;        //This means that The Stat Value is over the Above value
            else if (value.Value < Below) isBelow = true;   //This means that The Stat Value is under the Below value

            regenerate_LastValue = Regenerate;

            if (MaxValue < Value)
            {
                MaxValue = Value;
            }

            I_Regeneration = null;
            I_Degeneration = null;
            I_ModifyPerTicks = null;

            InmuneWait = new WaitForSeconds(InmuneTime);

            //        Debug.Log(Name + " MAX: " + MaxValue + "Val: " + Value);

            OnValueChangeNormalized.Invoke(NormalizedValue);
            OnValueChange.Invoke(Value);

            if (Active)
            {
                StartRegeneration();
                StartDegeneration();
            }
        }

        internal void SetMultiplier(float value)
        {
            multiplier.Value = value;
        }
     

        internal void SetValue(float value)
        {
            this.value.Value = value * Multiplier;

            if (this.value <= minValue.Value)
            {
                this.value.Value = minValue.Value;
                OnStatEmpty.Invoke();   //if the Value is 0 invoke Empty Stat
            }
            else if (this.value >= maxValue.Value)
            {
                this.value.Value = maxValue.Value;
                OnStatFull.Invoke();    //if the Value is 0 invoke Empty Stat
            }


            OnValueChangeNormalized.Invoke(this.value / MaxValue);
            OnValueChange.Invoke(this.value);

            if (this.value > Above && !isAbove)
            {
                OnStatAbove.Invoke();
                isAbove = true;
                isBelow = false;
            }
            else if (this.value < Below && !isBelow)
            {
                OnStatBelow.Invoke();
                isBelow = true;
                isAbove = false;
            }
        }

        /// <summary>Enable or Disable a Stat </summary>
        public void SetActive(bool enable)  { Active = enable; }
        public void SetRegeneration(bool enable)  { Regenerate = enable; }
        public void SetDegeneration(bool enable)  { Degenerate = enable; }

        /// <summary>Adds or remove to the Stat Value </summary>
        public virtual void Modify(float newValue)
        {
            if (!IsInmune && Active)
            {
                Value += newValue;
                StartRegeneration();
                if (!Regenerate) 
                    StartDegeneration();

                SetInmune();
            }
        }

        public virtual void UpdateStat()
        {
            SetValue(value);
            StartRegeneration();
            if (!Regenerate)
                StartDegeneration();
        }

        /// <summary>Adds or remove to the Stat Value</summary>
        public virtual void Modify(float newValue, float time)
        { 
            if (!IsInmune && Active)
            {
                StopSlowModification();
                I_ModifySlow = C_SmoothChangeValue(newValue, time);
                Coroutine.StartCoroutine(I_ModifySlow);
                SetInmune();
            }
        }

        /// <summary>
        /// Modify the Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds
        /// </summary>
        public virtual void Modify(float newValue, int ticks, float timeBetweenTicks)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            if (I_ModifyPerTicks != null)
                Coroutine.StopCoroutine(I_ModifyPerTicks);

            I_ModifyPerTicks = C_ModifyTicksValue(newValue, ticks, timeBetweenTicks);
            Coroutine.StartCoroutine(I_ModifyPerTicks);
        }

        /// <summary> Add or Remove Value the 'MaxValue' of the Stat </summary>
        public virtual void ModifyMAX(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable
            MaxValue += newValue;
            StartRegeneration();
        }

        /// <summary>Sets the 'MaxValue' of the Stat </summary>
        public virtual void SetMAX(float newValue)
        {
            if (!Active) return;    
            MaxValue = newValue;
            StartRegeneration();
        }


        /// <summary>Add or Remove Rate to the Regeneration Rate</summary>
        public virtual void ModifyRegenRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenRate.Value += newValue;
            StartRegeneration();
        }

        public virtual void SetRegenerationWait(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenWaitTime.Value = newValue;

            if (RegenWaitTime < 0) RegenWaitTime.Value = 0;
        }

        /// <summary>Set a new Regeneration Rate</summary>
        public virtual void SetRegenerationRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable
            RegenRate.Value = newValue;
        }

        /// <summary> Reset the Stat to the Default Min or Max Value</summary>
        public virtual void Reset()
        {
            Value = resetTo == ResetTo.MaxValue ? Value = MaxValue : Value = MinValue;
        } 
        /// <summary>Clean all Coroutines</summary>
        internal void CleanRoutines()
        {
            StopDegeneration();
            StopRegeneration();
            StopTickDamage();
            StopSlowModification();
        }


        public virtual void RegenerateOverTime(float time)
        {
            if (time <= 0)
            {
                StartRegeneration();
            }
            else
            {
                Coroutine.StartCoroutine(C_RegenerateOverTime(time));
            }
        }

        protected virtual void SetInmune()
        {
            if (InmuneTime > 0)
            {
                if (I_IsInmune != null)
                    Coroutine.StopCoroutine(I_IsInmune);


                Coroutine.StartCoroutine(I_IsInmune = C_InmuneTime());
            }
        }


        protected virtual void StartRegeneration()
        {
            StopRegeneration();

            if (RegenRate == 0 || !Regenerate) return;            //Means if there's no Regeneration

            Coroutine.StartCoroutine(I_Regeneration = C_Regenerate());
        }


        protected virtual void StartDegeneration()
        {
            StopDegeneration();
            if (DegenRate == 0 || !Degenerate) return;                       //Means there's no Degeneration

            Coroutine.StartCoroutine(I_Degeneration = C_Degenerate());
        }

        protected virtual void StopRegeneration()
        {
            if (I_Regeneration != null)  
                Coroutine.StopCoroutine(I_Regeneration);    //If there was a regenation active .... interrupt it
              
            I_Regeneration = null;
            IsRegenerating = false;
        }

        protected virtual void StopDegeneration()
        {
            if (I_Degeneration != null)  
                Coroutine.StopCoroutine(I_Degeneration);    //if it was ALREADY Degenerating.. stop
              
            I_Degeneration = null;
            IsDegenerating = false;
        }

        protected virtual void StopTickDamage()
        {
            if (I_ModifyPerTicks != null)
                Coroutine.StopCoroutine(I_ModifyPerTicks);   //if it was ALREADY Degenerating.. stop...

            I_ModifyPerTicks = null;
        }

        protected virtual void StopSlowModification()
        {
            if (I_ModifySlow != null)
                Coroutine.StopCoroutine(I_ModifySlow);       //If there was a regenation active .... interrupt it
            I_ModifySlow = null;
        }

        #region Coroutines
        private MonoBehaviour Coroutine;        //I need this to use coroutines in this class because it does not inherit from Monobehaviour
        private IEnumerator I_Regeneration;
        private IEnumerator I_Degeneration;
        private IEnumerator I_ModifyPerTicks;
        private IEnumerator I_ModifySlow;
        private IEnumerator I_IsInmune;


        protected IEnumerator C_RegenerateOverTime(float time)
        {
            float ReachValue = RegenRate > 0 ? MaxValue : MinValue;                                //Set to the default or 0
            bool Positive = RegenRate > 0;                                                          //Is the Regeneration Positive?
            float currentTime = Time.time;

            while (Value != ReachValue || currentTime > time )
            {
                Value += (RegenRate * Time.deltaTime);

                if (Positive && Value > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (!Positive && Value < 0)
                {
                    Value = MinValue;
                }
                currentTime += Time.deltaTime;

                yield return null;
            }
            yield return null;
        }

        protected IEnumerator C_InmuneTime()
        {
            IsInmune = true;
            yield return InmuneWait;
            IsInmune = false;
        }

        protected IEnumerator C_Regenerate()
        {
            if (RegenWaitTime > 0)
                yield return new WaitForSeconds(RegenWaitTime);          //Wait a time to regenerate

            IsRegenerating = true;

            while (Regenerate && Value < MaxValue)
            {
                Value += (RegenRate * Time.deltaTime);
                yield return null;
            }

            IsRegenerating = false;
            yield return null;
        }

        protected IEnumerator C_Degenerate()
        {
            if (DegenWaitTime > 0)
                yield return new WaitForSeconds(DegenWaitTime);          //Wait a time to regenerate


            IsDegenerating = true;

            while (Degenerate && Value > MinValue)
            {
                Value -= (DegenRate * Time.deltaTime);
                yield return null;
            }
            IsDegenerating = false;
            yield return null;
        }

        protected IEnumerator C_ModifyTicksValue(float value, int Ticks, float time)
        {
            var WaitForTicks = new WaitForSeconds(time);

            for (int i = 0; i < Ticks; i++)
            {
                Value += value;
                if (Value <= MinValue)
                {
                    Value = MinValue;
                    break;
                }
                yield return WaitForTicks;
            }

            yield return null;

            StartRegeneration();
        }

        protected IEnumerator C_SmoothChangeValue(float newvalue, float time)
        {
            StopRegeneration();

            Debug.Log(newvalue);

            float currentTime = 0;
            float currentValue = Value;
            newvalue = Value + newvalue;


            while (currentTime <= time)
            {

                Value = Mathf.Lerp(currentValue, newvalue, currentTime / time);
                currentTime += Time.deltaTime;


                yield return null;
            }
            Value = newvalue;

            yield return null;
            StartRegeneration();
        }
        #endregion

        public enum ResetTo
        {
            MinValue,
            MaxValue
        }
    }
}