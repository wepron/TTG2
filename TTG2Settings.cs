using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace TTG2
{
    // =========================================================================
    // НАСТРОЙКИ (MCM)
    // =========================================================================
    //
    // Глобальные настройки (не привязаны к сейву), сохраняются MCM в JSON.
    // Минимум — 1 (ванильное поведение). Максимум — 10000. Реально безопасный
    // диапазон с новой формулой — весь, вплоть до 10000. Переполнения не будет,
    // потому что целевое золото ограничено int.MaxValue - 1.
    // =========================================================================

    public class TTG2Settings : AttributeGlobalSettings<TTG2Settings>
    {
        public override string Id => "TTG2Settings";
        public override string DisplayName => "TTG2 – Town Gold Multiplier";
        public override string FormatType => "json";

        // --- Вкл / Выкл ---
        private bool _enableMod = true;

        [SettingPropertyGroup("General", GroupOrder = 0)]
        [SettingPropertyBool(
            "Enable Mod",
            Order = 0,
            RequireRestart = false,
            HintText = "Главный выключатель TTG2. Если выключено, золото в городах работает как в ванилле.")]
        public bool EnableMod
        {
            get => _enableMod;
            set
            {
                if (_enableMod != value)
                {
                    _enableMod = value;
                    OnPropertyChanged(nameof(EnableMod));
                }
            }
        }

        // --- Множитель целевого золота ---
        private int _goldMultiplier = 1;

        [SettingPropertyGroup("General", GroupOrder = 0)]
        [SettingPropertyInteger(
            "Gold Multiplier",
            1,
            10000,
            Order = 1,
            RequireRestart = false,
            HintText = "Во сколько раз увеличить ЦЕЛЕВОЕ золото города. 1 = ванилла. " +
                       "50 = города будут стремиться к 50-кратному запасу. " +
                       "1000+ — безопасно, переполнения не будет.")]
        public int GoldMultiplier
        {
            get => _goldMultiplier;
            set
            {
                if (_goldMultiplier != value)
                {
                    _goldMultiplier = value;
                    OnPropertyChanged(nameof(GoldMultiplier));
                }
            }
        }
    }
}