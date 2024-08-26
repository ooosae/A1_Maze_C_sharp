using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

using CaveCore;

using CommonCore;

using MazeCore;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using ReactiveUI;
namespace MazeDesktop.ViewModels;

public class MainViewModel : ViewModelBase {
  // private const string OpenFileErrorMsg = "File cannot be read correctly.";

  private Window _window;
  private CaveGenerator _caveGenerator;
  private Timer _timer;

#region Maze
  private Maze _maze;
  public Maze MazePuzzle {
    get => _maze;
    set => this.RaiseAndSetIfChanged(ref _maze, value);
  }
#endregion
#region RowsCount
  private int _rowsCount = 5;
  public int RowsCount {
    get => _rowsCount;
    set => this.RaiseAndSetIfChanged(ref _rowsCount, value);
  }
#endregion
#region CollsCount
  private int _collsCount = 5;
  public int ColsCount {
    get => _collsCount;
    set => this.RaiseAndSetIfChanged(ref _collsCount, value);
  }
#endregion
#region StartRow
  private int _startRow = 1;
  public int StartRow {
    get => _startRow;
    set => this.RaiseAndSetIfChanged(ref _startRow, value);
  }
#endregion
#region StartCol
  private int _startCol = 1;
  public int StartCol {
    get => _startCol;
    set => this.RaiseAndSetIfChanged(ref _startCol, value);
  }
#endregion
#region FinishRow
  private int _finishRow = 2;
  public int FinishRow {
    get => _finishRow;
    set => this.RaiseAndSetIfChanged(ref _finishRow, value);
  }
#endregion
#region FinishCol
  private int _finishCol = 2;
  public int FinishCol {
    get => _finishCol;
    set => this.RaiseAndSetIfChanged(ref _finishCol, value);
  }
#endregion
#region Route
  private List<Cell> _route;
  public List<Cell> Route {
    get => _route;
    set => this.RaiseAndSetIfChanged(ref _route, value);
  }
#endregion

#region MazeCommands
  public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

  public ReactiveCommand<Unit, Unit> GenerateMazeCommand { get; }

  public ReactiveCommand<Unit, Unit> SaveMazeCommand { get; }

  public ReactiveCommand<Unit, Unit> SolveMazeCommand { get; }
#endregion

#region Cave
  private Cave _cave;
  public Cave Cave {
    get => _cave;
    set => this.RaiseAndSetIfChanged(ref _cave, value);
  }
#endregion
#region CaveCollsCount
  private int _caveRowsCount = 50;
  public int CaveCollsCount {
    get => _caveCollsCount;
    set => this.RaiseAndSetIfChanged(ref _caveCollsCount, value);
  }
#endregion
#region CaveRowsCount
  private int _caveCollsCount = 50;
  public int CaveRowsCount {
    get => _caveRowsCount;
    set => this.RaiseAndSetIfChanged(ref _caveRowsCount, value);
  }
#endregion
#region BirthLimit
  private int _birthLimit = 5;
  public int BirthLimit {
    get => _birthLimit;
    set => this.RaiseAndSetIfChanged(ref _birthLimit, value);
  }
#endregion
#region DeathLimit
  private int _deathLimit = 4;
  public int DeathLimit {
    get => _deathLimit;
    set => this.RaiseAndSetIfChanged(ref _deathLimit, value);
  }
#endregion
#region BirthChance

  private decimal _birthChance = 0.55m;
  public decimal BirthChance {
    get => _birthChance;
    set => this.RaiseAndSetIfChanged(ref _birthChance, value);
  }
#endregion
#region TimeInterval
  private int _timeInterval = 500;
  public int TimeInterval {
    get => _timeInterval;
    set => this.RaiseAndSetIfChanged(ref this._timeInterval, value);
  }
#endregion

#region CaveCommands
  public ReactiveCommand<Unit, Unit> OpenCaveButtonCommand { get; }

  public ReactiveCommand<Unit, Unit> InitCaveCommand { get; }

  public ReactiveCommand<Unit, Unit> AutoGenerateCaveCommand { get; }

  public ReactiveCommand<Unit, Unit> NextGenerationStepCommand { get; }
#endregion

  public MainViewModel(Window window) {
    _window = window;

    OpenFileCommand = ReactiveCommand.CreateFromTask(OpenMazeAsync);
    GenerateMazeCommand = ReactiveCommand.CreateFromTask(GenerateMazeAsync);
    SaveMazeCommand = ReactiveCommand.CreateFromTask(SaveMazeAsync);
    SolveMazeCommand = ReactiveCommand.CreateFromTask(SolveMazeAsync);

    OpenCaveButtonCommand = ReactiveCommand.CreateFromTask(OpenCaveAsync);

    InitCaveCommand = ReactiveCommand.CreateFromTask(InitCaveAsync);
    NextGenerationStepCommand = ReactiveCommand.CreateFromTask(NextGenerationStepAsync);
    AutoGenerateCaveCommand = ReactiveCommand.CreateFromTask(GenerateCaveAsync);
  }

  private async Task OpenMazeAsync() {
    var filePath = await SelectFile();

    try {
      if (filePath is null) {
        return;
      }
      MazeTxtReader _mazeReader = new(filePath);
      MazePuzzle = _mazeReader.Read();

      if (MazePuzzle is null) {
        return;
      }
      RowsCount = MazePuzzle.RowsCount;
      ColsCount = MazePuzzle.ColsCount;
      Route = null;
    } catch {
      await ShowOpenFileErrorMessageBox();
    }
  }

  private async Task GenerateMazeAsync() {
    EllersMazeGenerator _mazeCreator = new EllersMazeGenerator(new Size(RowsCount, ColsCount));
    MazePuzzle = _mazeCreator.Create();
    Route = null;
  }

  private async Task SaveMazeAsync() {
    if (MazePuzzle is null) {
      return;
    }

    var file = await _window.StorageProvider.SaveFilePickerAsync(
        new FilePickerSaveOptions { Title = "Save MazePuzzle as Text File",
                                    DefaultExtension = ".txt",
                                    FileTypeChoices = [FilePickerFileTypes.TextPlain] });

    if (file is not null) {
      await using var stream = await file.OpenWriteAsync();
      using var streamWriter = new StreamWriter(stream);
      MazePuzzle.SaveToTxt(streamWriter);
    }
  }

  private async Task SolveMazeAsync() {
    if (MazePuzzle is not null) {
      StartRow = Math.Min(MazePuzzle.RowsCount, StartRow);
      StartCol = Math.Min(MazePuzzle.ColsCount, StartCol);
      FinishRow = Math.Min(MazePuzzle.RowsCount, FinishRow);
      FinishCol = Math.Min(MazePuzzle.ColsCount, FinishCol);

      Route = MazePuzzle.Solve(new Cell(StartRow - 1, StartCol - 1),
                               new Cell(FinishRow - 1, FinishCol - 1));
    }
  }

  private async Task OpenCaveAsync() {
    var filePath = await SelectFile();
    try {
      StopTimer();
      _caveGenerator = null;
      Cave = CaveTxtReader.Read(filePath);

      CaveRowsCount = Cave.RowsCount;
      CaveCollsCount = Cave.ColumnsCount;
    } catch {
      await ShowOpenFileErrorMessageBox();
    }
  }

  // private static async Task ShowOpenFileErrorMessageBox()
  //{
  //     var box = MessageBoxManager
  //                 .GetMessageBoxStandard("Caption", OpenFileErrorMsg,
  //                     ButtonEnum.Ok);

  //    var result = await box.ShowAsync();
  //}

  private async Task InitCaveAsync() {
    StopTimer();
    _caveGenerator = new CaveGenerator(new Size(CaveRowsCount, CaveCollsCount), BirthLimit,
                                       DeathLimit, (double)BirthChance);
    Cave = _caveGenerator.CurrentCave;
  }

  private async Task NextGenerationStepAsync() {
    StopTimer();

    if (_caveGenerator is null) {
      if (_cave is null) {
        return;
      } else {
        _caveGenerator = new CaveGenerator(Cave, BirthLimit, DeathLimit);
      }
    }

    Cave = _caveGenerator.GetNextGenerationCave();
  }

  private async Task GenerateCaveAsync() {
    StopTimer();

    if (_caveGenerator is null) {
      if (_cave is null) {
        return;
      } else {
        _caveGenerator = new CaveGenerator(Cave, BirthLimit, DeathLimit);
      }
    }

        TimerCallback tm = new((Object _) =>
        {
            if (_caveGenerator is null)
            {
                StopTimer();
  }
  Cave = _caveGenerator.GetNextGenerationCave();
  if (_caveGenerator.IsStable) {
    StopTimer();
  }
});

_timer = new(tm, null, _timeInterval, _timeInterval);
}

private void StopTimer() {
  if (_timer != null) {
    _timer.Dispose();
    _timer = null;
  }
}

// private async Task<string> SelectFile()
//{
//     var openFileDialog = new OpenFileDialog
//     {
//         Title = "Select a file",
//         AllowMultiple = false,
//         Filters = new List<FileDialogFilter>
//             {
//                 new FileDialogFilter
//                 {
//                     Name = "Text Documents",
//                     Extensions = new List<string> { "txt" }
//                 }
//             }
//     };

//    var result = await openFileDialog.ShowAsync(_window);

//    return result is null || result.Length == 0? null: result[0];
//}
}
