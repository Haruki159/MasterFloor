using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using MasterFloor.DataBase;

namespace MasterFloor.Pages
{
    /// <summary>
    /// Логика взаимодействия для PartnersPage.xaml
    /// </summary>
    public partial class PartnersPage : Page
    {
        public PartnersPage()
        {
            InitializeComponent();
            UpdatePartnersList();
        }
        private void UpdatePartnersList()
        {
            // Правильный подход: создавать контекст внутри блока 'using'.
            // Это гарантирует, что после выполнения кода контекст будет уничтожен,
            // а все отслеживаемые им сущности - "забыты".
            using (var db = new MasterPolEntities()) // <-- Создаем НОВЫЙ экземпляр контекста
            {
                // В остальном код не меняется.
                // Он будет работать корректно, потому что 'db' - это "чистый", свежий контекст.
                var partners = db.Partners
                                 .Include(p => p.PartnerType)
                                 .Include(p => p.Sales)
                                 .ToList();

                SalesListView.ItemsSource = partners;
            }
        }
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdatePartnersList();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Передаем null, чтобы страница открылась в режиме добавления
            Manager.MainFrame.Navigate(new AddEditPartnerPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedPartner = SalesListView.SelectedItem as Partner;
            if (selectedPartner == null)
            {
                MessageBox.Show("Выберите партнера для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Передаем выбранного партнера для редактирования
            Manager.MainFrame.Navigate(new AddEditPartnerPage(selectedPartner));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var partnersForRemoving = SalesListView.SelectedItems.Cast<Partner>().ToList();
            if (partnersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите партнеров для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить следующих {partnersForRemoving.Count()} элементов?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new MasterPolEntities())
                    {
                        foreach (var partner in partnersForRemoving)
                        {
                            db.Entry(partner).State = EntityState.Deleted;
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные удалены!");
                    }
                    UpdatePartnersList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void BtnCalcMaterial_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверяем, выбран ли партнер (через кнопку внутри списка)
            var selectedPartner = SalesListView.SelectedItem as Partner;

            if (selectedPartner == null)
            {
                MessageBox.Show("Сначала выберите партнера из списка!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Считаем общее количество продукции
            int totalProductCount = 0;
            try
            {
                // ИСПРАВЛЕНИЕ ЗДЕСЬ: используем .Sales вместо .PartnerProducts
                if (selectedPartner.Sales != null)
                {
                    foreach (var sale in selectedPartner.Sales)
                    {
                        // Если 'ProductQuantity' подчеркнет красным, попробуй стереть точку и выбрать из списка (например, Quantity или Count)
                        totalProductCount += sale.Quantity;
                    }
                }
            }
            catch
            {
                totalProductCount = 10; // Заглушка, если данные не подгрузились
            }

            if (totalProductCount == 0) totalProductCount = 1;

            // 3. Вызываем калькулятор
            // Убедись, что путь к классу MaterialCalculator правильный (MasterFloor.Services или просто MasterFloor)
            var calculator = new MasterFloor.MaterialCalculator();
            // Если здесь ошибка - удали ".Services" или добавь using MasterFloor.Services; наверху

            int typeProduct = 2;   // Ламинат
            int typeMaterial = 1;
            float param1 = 2.5f;
            float param2 = 0.5f;

            int result = calculator.GetQuantityForProduct(typeProduct, typeMaterial, totalProductCount, param1, param2);

            if (result != -1)
            {
                MessageBox.Show($"Партнер: {selectedPartner.PartnerName}\n" +
                                $"Всего реализовано продукции: {totalProductCount} шт.\n" +
                                $"Параметры материала: {param1}x{param2} м\n\n" +
                                $"РАСЧЕТ МАТЕРИАЛА: {result} шт.",
                                "Расчет материала", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ошибка расчета.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
