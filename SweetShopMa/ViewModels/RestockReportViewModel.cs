using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public class RestockReportViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private readonly Services.LocalizationService _localizationService;
    private bool _isBusy;

    public ObservableCollection<RestockRecordDisplay> RestockRecords { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand LoadCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public RestockReportViewModel(DatabaseService databaseService, Services.LocalizationService localizationService)
    {
        _databaseService = databaseService;
        _localizationService = localizationService;
        RefreshCommand = new Command(async () => await LoadRestockRecordsAsync());
        LoadCommand = new Command(async () => await LoadRestockRecordsAsync());
    }

    public async Task LoadRestockRecordsAsync()
    {
        IsBusy = true;
        try
        {
            var records = await _databaseService.GetRestockRecordsAsync();
            RestockRecords.Clear();
            foreach (var record in records)
            {
                RestockRecords.Add(new RestockRecordDisplay(record, _localizationService));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void RefreshLocalizedStrings()
    {
        var records = RestockRecords.ToList();
        RestockRecords.Clear();
        foreach (var display in records)
        {
            display.RefreshLocalizedStrings(_localizationService);
            RestockRecords.Add(display);
        }
    }

    protected virtual void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RestockRecordDisplay : INotifyPropertyChanged
{
    private readonly RestockRecord _record;
    private Services.LocalizationService _localizationService;

    public RestockRecordDisplay(RestockRecord record, Services.LocalizationService localizationService)
    {
        _record = record;
        _localizationService = localizationService;
    }

    public int Id => _record.Id;
    public int ProductId => _record.ProductId;
    public string ProductName => _record.ProductName;
    public string ProductEmoji => _record.ProductEmoji;
    public decimal QuantityAdded => _record.QuantityAdded;
    public decimal StockBefore => _record.StockBefore;
    public decimal StockAfter => _record.StockAfter;
    public int UserId => _record.UserId;
    public string UserName => _record.UserName;
    public DateTime RestockDate => _record.RestockDate;

    public string QuantityDisplay => $"{_localizationService.GetString("Quantity")}: {QuantityAdded}";
    public string StockBeforeDisplay => $"{_localizationService.GetString("Before")}: {StockBefore}";
    public string StockAfterDisplay => $"{_localizationService.GetString("After")}: {StockAfter}";
    public string RestockedByDisplay => $"{_localizationService.GetString("RestockedBy")}: {UserName}";

    public void RefreshLocalizedStrings(Services.LocalizationService localizationService)
    {
        _localizationService = localizationService;
        OnPropertyChanged(nameof(QuantityDisplay));
        OnPropertyChanged(nameof(StockBeforeDisplay));
        OnPropertyChanged(nameof(StockAfterDisplay));
        OnPropertyChanged(nameof(RestockedByDisplay));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

