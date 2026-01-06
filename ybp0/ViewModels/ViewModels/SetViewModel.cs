using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Timers;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class SetViewModel : BaseViewModel
    {
        private readonly IDatabaseService _dbService;
        private readonly int _workoutSessionId;
        private readonly int _exerciseId;
        private readonly Timer _saveTimer;

        private int _id;
        private int _setNumber;
        private string _reps;
        private string _weight;
        private string _setColor;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int SetNumber
        {
            get => _setNumber;
            set => SetProperty(ref _setNumber, value);
        }

        public string Reps
        {
            get => _reps;
            set
            {
                if (SetProperty(ref _reps, value))
                {
                    TriggerAutoSave();
                }
            }
        }

        public string Weight
        {
            get => _weight;
            set
            {
                if (SetProperty(ref _weight, value))
                {
                    TriggerAutoSave();
                }
            }
        }

        public string SetColor
        {
            get => _setColor;
            set => SetProperty(ref _setColor, value);
        }

        public SetViewModel(IDatabaseService dbService, int workoutSessionId, int exerciseId, string setColor = "#26A69A")
        {
            _dbService = dbService;
            _workoutSessionId = workoutSessionId;
            _exerciseId = exerciseId;
            _setColor = setColor;

            // Debounce timer for auto-save (500ms delay)
            _saveTimer = new Timer(500);
            _saveTimer.AutoReset = false; // Only fire once per trigger
            _saveTimer.Elapsed += SaveTimer_Elapsed;
        }

        private void TriggerAutoSave()
        {
            // Reset timer on each change (debounce)
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveToDatabase();
        }

        private void SaveToDatabase()
        {
            // Validate input
            if (!int.TryParse(Reps, out int reps) || !double.TryParse(Weight, out double weight))
            {
                return; // Don't save invalid data
            }

            try
            {
                _dbService.SaveSessionSet(_workoutSessionId, _exerciseId, SetNumber, reps, weight);
                System.Diagnostics.Debug.WriteLine($"Auto-saved set {SetNumber}: {reps} reps @ {weight}kg");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving set: {ex.Message}");
            }
        }
    }
}

