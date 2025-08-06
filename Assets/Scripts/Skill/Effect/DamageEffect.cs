
    public class DamageEffect : Effect
    {
        private float _defaultDamage;
        private float _mulAp;
        private float _mulAd;

        public DamageEffect(float defaultDamage, float mulAp, float mulAd) : base()
        {
            _defaultDamage = defaultDamage;
            _mulAp = mulAp;
            _mulAd = mulAd;
        }

        public override void Initialize(params object[] objects)
        {
            if(objects[0] is float defaultDamage) _defaultDamage = defaultDamage;
            if(objects[1] is float mulAp) _mulAp = mulAp;
            if(objects[2] is float mulAd) _mulAd = mulAd;
        }

        public override bool Apply(Actor source, Actor target)
        {
            float finalDamage = _defaultDamage + _defaultDamage + source.stat.ap * _mulAp + source.stat.ad * _mulAd;
            target.stat.health -= finalDamage - target.stat.defense;
            return true;
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }
    }