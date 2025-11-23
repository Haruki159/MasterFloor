using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFloor
{
    public class MaterialCalculator
    {
        /// <summary>
        /// Расчет количества материала с учетом брака
        /// </summary>
        /// <param name="productTypeIndex">Тип продукции (1, 2, 3...)</param>
        /// <param name="materialTypeIndex">Тип материала (процент брака)</param>
        /// <param name="count">Количество продукции</param>
        /// <param name="param1">Длина</param>
        /// <param name="param2">Ширина</param>
        /// <returns>Количество (шт) или -1 при ошибке</returns>
        public int GetQuantityForProduct(int productTypeIndex, int materialTypeIndex, int count, float param1, float param2)
        {
            // Проверка на дурака (отрицательные числа)
            if (count <= 0 || param1 <= 0 || param2 <= 0)
                return -1;

            // Коэффициенты типа продукции (из задания, проверь свои значения!)
            double productCoef = 0;
            switch (productTypeIndex)
            {
                case 1: productCoef = 1.1; break;
                case 2: productCoef = 2.5; break;
                case 3: productCoef = 8.43; break;
                default: return -1; // Неизвестный тип
            }

            // Процент брака (из задания)
            double defectRate = 0;
            switch (materialTypeIndex)
            {
                case 1: defectRate = 0.001; break; // 0.1%
                case 2: defectRate = 0.0095; break; // 0.95%
                // Добавь остальные, если есть
                default: return -1;
            }

            try
            {
                // Формула: (Площадь * Кол-во * Коэф) / (1 - %брака)
                double area = param1 * param2;
                double totalNeeded = (area * count * productCoef) / (1 - defectRate);

                // Округляем вверх до целого
                return (int)Math.Ceiling(totalNeeded);
            }
            catch
            {
                return -1;
            }
        }
    }
}
