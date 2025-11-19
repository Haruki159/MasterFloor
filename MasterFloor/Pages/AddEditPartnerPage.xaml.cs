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
using MasterFloor.DataBase;

namespace MasterFloor.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditPartnerPage.xaml
    /// </summary>
    public partial class AddEditPartnerPage : Page
    {
        private Partner _currentPartner;
        public AddEditPartnerPage(Partner selectedPartner)
        {
            InitializeComponent();
            if (selectedPartner == null)
            {
                // Режим добавления нового партнера
                _currentPartner = new Partner();
            }
            else
            {
                // Режим редактирования существующего партнера
                _currentPartner = selectedPartner;
            }

            // Устанавливаем контекст данных для привязок {Binding ...} в XAML
            DataContext = _currentPartner;
            // Загружаем типы партнеров в ComboBox
            LoadPartnerTypes();
        }

        private void LoadPartnerTypes()
        {
            using (var db = new MasterPolEntities())
            {
                ComboPartnerType.ItemsSource = db.PartnerTypes.ToList();
                // Если это редактирование, нужно выбрать правильный тип в ComboBox
                if (_currentPartner.PartnerType != null)
                {
                    ComboPartnerType.SelectedItem = _currentPartner.PartnerType;
                }
            }
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных (остается без изменений)
            var errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_currentPartner.PartnerName))
                errors.AppendLine("Укажите наименование партнера");
            if (ComboPartnerType.SelectedItem == null)
                errors.AppendLine("Выберите тип партнера");
            if (string.IsNullOrWhiteSpace(_currentPartner.INN))
                errors.AppendLine("Укажите ИНН");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сохранение в БД
            using (var db = new MasterPolEntities())
            {
                // Получаем выбранный тип из ComboBox
                var selectedPartnerType = ComboPartnerType.SelectedItem as PartnerType;

                if (_currentPartner.PartnerID == 0) // Если это НОВЫЙ партнер
                {
                    // 1. Устанавливаем навигационное свойство
                    _currentPartner.PartnerType = selectedPartnerType;
                    // 2. Явно "прикрепляем" связанный объект PartnerType к новому контексту.
                    //    Это говорит EF: "Этот PartnerType уже существует, не надо его создавать".
                    db.PartnerTypes.Attach(_currentPartner.PartnerType);
                    // 3. Добавляем сам НОВЫЙ объект Partner
                    db.Partners.Add(_currentPartner);
                }
                else // Если это РЕДАКТИРОВАНИЕ существующего партнера
                {
                    // 1. Прикрепляем основной объект к контексту
                    var existingPartner = db.Partners.Find(_currentPartner.PartnerID);
                    if (existingPartner != null)
                    {
                        // 2. Обновляем его свойства из нашей формы (_currentPartner)
                        db.Entry(existingPartner).CurrentValues.SetValues(_currentPartner);
                        // 3. Обновляем связанный тип партнера
                        existingPartner.PartnerTypeID = selectedPartnerType.PartnerTypeID;
                    }
                }

                try
                {
                    db.SaveChanges();
                    MessageBox.Show("Информация сохранена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Manager.MainFrame.GoBack(); // Возвращаемся на предыдущую страницу
                }
                catch (Exception ex)
                {
                    // Наш улучшенный обработчик ошибок
                    string errorMessage = ex.Message;
                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        errorMessage += $"\n\n--> {innerException.Message}";
                        innerException = innerException.InnerException;
                    }
                    MessageBox.Show($"Ошибка при сохранении:\n{errorMessage}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
