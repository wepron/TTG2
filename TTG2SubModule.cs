using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade;

namespace TTG2
{
    // =========================================================================
    // SUBMODULE
    // =========================================================================
    //
    // Задача: дать возможность выставлять огромные множители золота в городах
    // (сотни, тысячи) без риска переполнения int и без экспоненциального
    // расхождения, как было в мультипликативном подходе.
    //
    // Решение: вместо умножения ежедневного прироста (delta) мы переопределяем
    // ЦЕЛЕВОЕ золото города. Игра, как и в ванилле, продолжает плавно двигать
    // золото к цели — но цель теперь масштабируется множителем.
    //
    // Формула (новая):
    //   baseTarget = 10000 + Prosperity * 12
    //   target     = clamp(baseTarget * M, 0, int.MaxValue - 1)
    //   delta      = 0.25 * (target - Gold)
    //
    // Вся арифметика ведётся в double, чтобы промежуточные значения не
    // переполняли int. Финальный результат уже безопасно приводится к int.
    // =========================================================================

    public class TTG2SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                var harmony = new Harmony("com.ttg2.patch");
                harmony.PatchAll();
            }
            catch
            {
                // Если Harmony не загружен или патч не удался — мод просто
                // не активируется. Игра не падает.
            }
        }
    }

    // =========================================================================
    // ПАТЧ: DefaultSettlementEconomyModel.GetTownGoldChange
    // =========================================================================
    //
    // Ванильный метод:
    //   float num = 10000f + town.Prosperity * 12f - (float)town.Gold;
    //   return MathF.Round(0.25f * num);
    //
    // Мы перехватываем его Prefix'ом и, если мод включён и множитель > 1,
    // полностью заменяем результат. Если мод выключен, множитель = 1 или
    // что-то пошло не так — возвращаем true, и игра выполняет ванильную
    // логику. Это гарантирует, что при любом сбое поведение остаётся
    // максимально близким к оригиналу.
    // =========================================================================

    [HarmonyPatch(typeof(DefaultSettlementEconomyModel), "GetTownGoldChange")]
    public static class Patch_GetTownGoldChange
    {
        static bool Prefix(Town town, ref int __result)
        {
            try
            {
                // 1. Настройки
                var settings = TTG2Settings.Instance;
                if (settings == null || !settings.EnableMod)
                    return true;

                // 2. Множитель должен быть > 1, иначе нет смысла вмешиваться
                int multiplier = settings.GoldMultiplier;
                if (multiplier <= 1)
                    return true;

                // 3. Контекст
                if (town == null || Campaign.Current == null)
                    return true;

                // 4. Считаем целевую точку в double, чтобы не переполниться
                double baseTarget = 10000.0 + (double)town.Prosperity * 12.0;
                double target = baseTarget * multiplier;

                // 5. Мягкий потолок, чтобы целевое золото не выходило за int
                const double safeMax = (double)int.MaxValue - 1.0;
                if (target > safeMax) target = safeMax;
                if (target < 0.0) target = 0.0;

                // 6. Ежедневное изменение — та же формула, что и в ванилле,
                //    но с новой целью
                double delta = 0.25 * (target - (double)town.Gold);

                // 7. На всякий случай ограничиваем дельту диапазоном int
                if (delta > int.MaxValue) delta = int.MaxValue;
                if (delta < int.MinValue) delta = int.MinValue;

                __result = (int)Math.Round(delta, MidpointRounding.AwayFromZero);

                // 8. Пропускаем оригинальный метод — мы уже посчитали всё сами
                return false;
            }
            catch
            {
                // Любая ошибка — возвращаемся к ванильному поведению
                return true;
            }
        }
    }
}