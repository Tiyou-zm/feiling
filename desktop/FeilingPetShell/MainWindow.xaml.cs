using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FeilingPetShell;

public partial class MainWindow : Window
{
    private enum PetMovementMode
    {
        Pinned,
        Wander,
        FollowMouse
    }

    private enum PetAnimationMode
    {
        BaseIdle,
        IdleLoop,
        WalkLoop
    }

    private enum PetFacingDirection
    {
        Right = 1,
        Left = -1
    }

    private const double TargetPetDisplayHeight = 330;
    private const double RightMenuReserveWidth = 40;
    private const double WindowHorizontalPadding = 6;
    private const double WindowVerticalPadding = 6;
    private const byte AlphaThreshold = 8;
    private const double WanderSpeedPixelsPerSecond = 96;
    private const double FollowSpeedPixelsPerSecond = 152;
    private const double FollowMouseDeadZone = 86;
    private const double WanderArrivalThreshold = 10;
    private const double ScreenPadding = 8;
    private const double FacingChangeThreshold = 0.5;
    private const int SourceSpriteFacingScaleX = 1;

    private readonly string _projectRoot;
    private readonly DispatcherTimer _speechTimer;
    private readonly DispatcherTimer _idleLoopTimer;
    private readonly DispatcherTimer _movementTimer;
    private readonly DispatcherTimer _blinkIntervalTimer;
    private readonly DispatcherTimer _blinkFrameTimer;
    private readonly SpeechBubbleWindow _speechWindow;
    private readonly List<BitmapSource> _idleLoopFrames = new();
    private readonly List<BitmapSource> _walkLoopFrames = new();
    private readonly List<BitmapSource?> _blinkFrames = new();
    private readonly string[] _greetingLines =
    {
        "哼，我本来就在这边陪着你。",
        "你叫我，我就应一声。不是特意等你哦。",
        "今天也别把桌面弄得太乱，我会看着你的。",
        "要发呆也行，我先陪着你。",
        "真拿你没办法，我会在这边守着的。",
        "先慢慢做，我就在旁边。"
    };

    private Point _dragStartScreen;
    private Point _dragStartWindow;
    private bool _pointerDown;
    private bool _dragging;
    private readonly Random _random = new();
    private double _petDisplayWidth;
    private double _petDisplayHeight;
    private PetMovementMode _movementMode = PetMovementMode.Pinned;
    private Point? _movementTarget;
    private DateTime _lastMovementTickUtc;
    private DateTime _wanderPauseUntilUtc = DateTime.MinValue;
    private int _blinkFrameIndex = -1;
    private int _idleLoopFrameIndex;
    private int _idleLoopDirection = 1;
    private int _walkLoopFrameIndex;
    private bool _useIdleLoop;
    private PetAnimationMode _animationMode = PetAnimationMode.BaseIdle;
    private PetFacingDirection _facingDirection = PetFacingDirection.Right;

    public MainWindow()
    {
        InitializeComponent();

        _projectRoot = ResolveProjectRoot();
        _speechWindow = new SpeechBubbleWindow();
        _speechTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.2) };
        _idleLoopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(92) };
        _movementTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _blinkIntervalTimer = new DispatcherTimer();
        _blinkFrameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(115) };
        _speechTimer.Tick += (_, _) =>
        {
            _speechTimer.Stop();
            _speechWindow.Hide();
        };
        _idleLoopTimer.Tick += (_, _) => AdvanceIdleLoopFrame();
        _movementTimer.Tick += (_, _) => AdvanceMovement();
        _blinkIntervalTimer.Tick += (_, _) => StartBlinkSequence();
        _blinkFrameTimer.Tick += (_, _) => AdvanceBlinkFrame();

        LoadIdleLoopFrames();
        LoadWalkLoopFrames();
        LoadPetImage();
        LoadBlinkFrames();
        Loaded += OnLoaded;
        LocationChanged += (_, _) => UpdateSpeechWindowPosition();
        SizeChanged += (_, _) => UpdateSpeechWindowPosition();
        Closed += (_, _) =>
        {
            _speechTimer.Stop();
            _idleLoopTimer.Stop();
            _movementTimer.Stop();
            _blinkIntervalTimer.Stop();
            _blinkFrameTimer.Stop();
            _speechWindow.Close();
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 22;
        Top = workArea.Bottom - Height - 18;
        UpdateSpeechWindowPosition();
        _lastMovementTickUtc = DateTime.UtcNow;
        _movementTimer.Start();
        UpdateMovementUi();
        ApplyFacingDirection();
        SetAnimationMode(GetIdleAnimationMode());
        if (_useIdleLoop || _walkLoopFrames.Count > 0)
        {
            _idleLoopTimer.Start();
        }
        else
        {
            ScheduleNextBlink();
        }
    }

    private void LoadPetImage()
    {
        BitmapSource sourceBitmap;
        if (_idleLoopFrames.Count > 0)
        {
            sourceBitmap = _idleLoopFrames[0];
            _useIdleLoop = true;
        }
        else
        {
            var imagePath = Path.Combine(_projectRoot, "assets", "characters", "feiling", "base", "feiling_master_v1.png");
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Feiling asset not found: {imagePath}");
            }

            sourceBitmap = CropToVisibleBounds(LoadBitmap(imagePath));
            _useIdleLoop = false;
        }

        var croppedBitmap = sourceBitmap;
        croppedBitmap.Freeze();

        var scale = TargetPetDisplayHeight / croppedBitmap.PixelHeight;
        _petDisplayWidth = Math.Round(croppedBitmap.PixelWidth * scale);
        _petDisplayHeight = Math.Round(croppedBitmap.PixelHeight * scale);

        PetImage.Source = croppedBitmap;
        PetImage.Width = _petDisplayWidth;
        PetImage.Height = _petDisplayHeight;
        BlinkOverlayImage.Width = _petDisplayWidth;
        BlinkOverlayImage.Height = _petDisplayHeight;
        PetDragSurface.Width = _petDisplayWidth;
        PetDragSurface.Height = _petDisplayHeight;

        Width = _petDisplayWidth + RightMenuReserveWidth + WindowHorizontalPadding;
        Height = _petDisplayHeight + WindowVerticalPadding;
        MinWidth = Width;
        MaxWidth = Width;
        MinHeight = Height;
        MaxHeight = Height;
    }

    private void LoadIdleLoopFrames()
    {
        var idleLoopDir = Path.Combine(_projectRoot, "assets", "characters", "feiling", "animations", "idle_loop");
        if (!Directory.Exists(idleLoopDir))
        {
            return;
        }

        _idleLoopFrames.Clear();
        foreach (var path in Directory.GetFiles(idleLoopDir, "feiling_idle_loop_*.png").OrderBy(path => path))
        {
            // idle_loop frames were already cropped once with a shared union alpha box.
            // Re-cropping each frame individually here causes the visible size to pulse.
            var frame = LoadBitmap(path);
            frame.Freeze();
            _idleLoopFrames.Add(frame);
        }
    }

    private void LoadWalkLoopFrames()
    {
        var walkLoopDir = Path.Combine(_projectRoot, "assets", "characters", "feiling", "animations", "walk_loop");
        if (!Directory.Exists(walkLoopDir))
        {
            return;
        }

        _walkLoopFrames.Clear();
        foreach (var path in Directory.GetFiles(walkLoopDir, "feiling_walk_loop_*.png").OrderBy(path => path))
        {
            var frame = LoadBitmap(path);
            frame.Freeze();
            _walkLoopFrames.Add(frame);
        }
    }

    private void LoadBlinkFrames()
    {
        if (_useIdleLoop)
        {
            _blinkFrames.Clear();
            return;
        }

        var animationDir = Path.Combine(_projectRoot, "assets", "characters", "feiling", "animations");
        var framePaths = new[]
        {
            Path.Combine(animationDir, "feiling_idle_blink_half_overlay_v3.png"),
            Path.Combine(animationDir, "feiling_idle_blink_closed_overlay_v3.png"),
            Path.Combine(animationDir, "feiling_idle_blink_half_return_overlay_v1.png"),
            string.Empty
        };

        _blinkFrames.Clear();
        foreach (var path in framePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _blinkFrames.Add(null);
                continue;
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Blink overlay not found: {path}");
            }

            _blinkFrames.Add(LoadBitmap(path));
        }
    }

    private static BitmapSource LoadBitmap(string imagePath)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static BitmapSource CropToVisibleBounds(BitmapSource source)
    {
        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        converted.Freeze();

        var stride = converted.PixelWidth * 4;
        var pixels = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(pixels, stride, 0);

        var minX = converted.PixelWidth;
        var minY = converted.PixelHeight;
        var maxX = -1;
        var maxY = -1;

        for (var y = 0; y < converted.PixelHeight; y++)
        {
            var rowOffset = y * stride;
            for (var x = 0; x < converted.PixelWidth; x++)
            {
                var alpha = pixels[rowOffset + (x * 4) + 3];
                if (alpha <= AlphaThreshold)
                {
                    continue;
                }

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0 || maxY < 0)
        {
            return converted;
        }

        var cropRect = new Int32Rect(
            minX,
            minY,
            maxX - minX + 1,
            maxY - minY + 1);

        return new CroppedBitmap(converted, cropRect);
    }

    private static string ResolveProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "assets")) &&
                Directory.Exists(Path.Combine(current.FullName, "scripts")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not resolve feiling project root.");
    }

    private static Rect GetUsableWorkArea()
    {
        var area = SystemParameters.WorkArea;
        return new Rect(
            area.Left + ScreenPadding,
            area.Top + ScreenPadding,
            Math.Max(0, area.Width - (ScreenPadding * 2)),
            Math.Max(0, area.Height - (ScreenPadding * 2)));
    }

    private void SetMovementMode(PetMovementMode mode)
    {
        _movementMode = mode;
        _movementTarget = null;
        _wanderPauseUntilUtc = mode == PetMovementMode.Wander
            ? DateTime.UtcNow.AddMilliseconds(_random.Next(1200, 2600))
            : DateTime.MinValue;
        UpdateMovementUi();
        SetAnimationMode(GetIdleAnimationMode());
    }

    private void UpdateMovementUi()
    {
        MovementModeText.Text = _movementMode switch
        {
            PetMovementMode.Wander => "当前：闲逛中",
            PetMovementMode.FollowMouse => "当前：追逐鼠标",
            _ => "当前：固定位置"
        };

        ApplyModeButtonVisual(WanderButton, _movementMode == PetMovementMode.Wander);
        ApplyModeButtonVisual(FollowMouseButton, _movementMode == PetMovementMode.FollowMouse);
        ApplyModeButtonVisual(PinPositionButton, _movementMode == PetMovementMode.Pinned);
    }

    private static void ApplyModeButtonVisual(System.Windows.Controls.Button button, bool active)
    {
        button.Background = active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD35F67"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF6EEE6"));
        button.BorderBrush = active
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#30AF705F"));
        button.Foreground = active
            ? Brushes.White
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7A5345"));
    }

    private void AdvanceMovement()
    {
        var nowUtc = DateTime.UtcNow;
        var elapsed = (nowUtc - _lastMovementTickUtc).TotalSeconds;
        _lastMovementTickUtc = nowUtc;

        if (elapsed <= 0 || elapsed > 0.2)
        {
            return;
        }

        if (_pointerDown || _dragging || MenuPopup.IsOpen)
        {
            SetAnimationMode(GetIdleAnimationMode());
            return;
        }

        var movedThisTick = false;
        switch (_movementMode)
        {
            case PetMovementMode.Wander:
                movedThisTick = AdvanceWander(nowUtc, elapsed);
                break;
            case PetMovementMode.FollowMouse:
                movedThisTick = AdvanceFollowMouse(elapsed);
                break;
        }

        UpdateAnimationForMovement(movedThisTick);
    }

    private void UpdateAnimationForMovement(bool movedThisTick)
    {
        var desiredMode = movedThisTick ? GetWalkAnimationMode() : GetIdleAnimationMode();
        SetAnimationMode(desiredMode);
    }

    private PetAnimationMode GetIdleAnimationMode()
    {
        return _useIdleLoop ? PetAnimationMode.IdleLoop : PetAnimationMode.BaseIdle;
    }

    private PetAnimationMode GetWalkAnimationMode()
    {
        return _walkLoopFrames.Count > 0 ? PetAnimationMode.WalkLoop : GetIdleAnimationMode();
    }

    private void SetAnimationMode(PetAnimationMode mode)
    {
        if (mode == _animationMode)
        {
            return;
        }

        _animationMode = mode;
        switch (_animationMode)
        {
            case PetAnimationMode.WalkLoop:
                _walkLoopFrameIndex = 0;
                ApplyCurrentAnimationFrame();
                break;
            case PetAnimationMode.IdleLoop:
                _idleLoopFrameIndex = 0;
                _idleLoopDirection = 1;
                ApplyCurrentAnimationFrame();
                break;
            default:
                ApplyCurrentAnimationFrame();
                break;
        }
    }

    private bool AdvanceWander(DateTime nowUtc, double elapsed)
    {
        if (_movementTarget is null)
        {
            if (nowUtc < _wanderPauseUntilUtc)
            {
                return false;
            }

            _movementTarget = PickWanderTarget();
            return false;
        }

        var reached = MoveTowards(
            _movementTarget.Value,
            WanderSpeedPixelsPerSecond,
            elapsed,
            WanderArrivalThreshold,
            out var appliedDelta);
        UpdateFacingDirectionFromMovement(appliedDelta);
        if (reached)
        {
            _movementTarget = null;
            _wanderPauseUntilUtc = nowUtc.AddMilliseconds(_random.Next(1800, 4800));
        }

        return DidMove(appliedDelta);
    }

    private bool AdvanceFollowMouse(double elapsed)
    {
        if (!TryGetCursorScreenPosition(out var cursor))
        {
            return false;
        }

        var target = new Point(
            cursor.X - (Width * 0.5),
            cursor.Y - (_petDisplayHeight * 0.72));

        var currentAnchor = new Point(Left + (Width * 0.5), Top + (_petDisplayHeight * 0.72));
        var distance = Distance(currentAnchor, cursor);
        if (distance < FollowMouseDeadZone)
        {
            return false;
        }

        MoveTowards(target, FollowSpeedPixelsPerSecond, elapsed, 6, out var appliedDelta);
        UpdateFacingDirectionFromMovement(appliedDelta);
        return DidMove(appliedDelta);
    }

    private Point PickWanderTarget()
    {
        var bounds = GetUsableWorkArea();
        var currentX = Left;
        var currentY = Top;
        var dx = _random.NextDouble() * 180 - 90;
        var dy = _random.NextDouble() * 80 - 40;
        var target = new Point(currentX + dx, currentY + dy);
        return ClampWindowPosition(target);
    }

    private bool MoveTowards(Point target, double speedPixelsPerSecond, double elapsed, double arrivalThreshold, out Vector appliedDelta)
    {
        appliedDelta = default;
        var current = new Point(Left, Top);
        var dx = target.X - current.X;
        var dy = target.Y - current.Y;
        var distance = Math.Sqrt((dx * dx) + (dy * dy));
        if (distance <= arrivalThreshold)
        {
            var snapped = ClampWindowPosition(target);
            appliedDelta = new Vector(snapped.X - current.X, snapped.Y - current.Y);
            Left = snapped.X;
            Top = snapped.Y;
            UpdateSpeechWindowPosition();
            return true;
        }

        var step = Math.Min(distance, speedPixelsPerSecond * elapsed);
        if (step <= 0.01)
        {
            return false;
        }

        var next = new Point(
            current.X + (dx / distance * step),
            current.Y + (dy / distance * step));

        var clamped = ClampWindowPosition(next);
        appliedDelta = new Vector(clamped.X - current.X, clamped.Y - current.Y);
        Left = clamped.X;
        Top = clamped.Y;
        UpdateSpeechWindowPosition();
        return false;
    }

    private static bool DidMove(Vector appliedDelta)
    {
        return Math.Abs(appliedDelta.X) > 0.01 || Math.Abs(appliedDelta.Y) > 0.01;
    }

    private void UpdateFacingDirectionFromMovement(Vector appliedDelta)
    {
        if (Math.Abs(appliedDelta.X) < FacingChangeThreshold)
        {
            return;
        }

        _facingDirection = appliedDelta.X < 0
            ? PetFacingDirection.Left
            : PetFacingDirection.Right;
        ApplyFacingDirection();
    }

    private void ApplyFacingDirection()
    {
        PetScaleTransform.ScaleX = (int)_facingDirection * SourceSpriteFacingScaleX;
    }

    private Point ClampWindowPosition(Point point)
    {
        var bounds = GetUsableWorkArea();
        var maxX = Math.Max(bounds.Left, bounds.Right - Width);
        var maxY = Math.Max(bounds.Top, bounds.Bottom - Height);
        var x = Math.Max(bounds.Left, Math.Min(maxX, point.X));
        var y = Math.Max(bounds.Top, Math.Min(maxY, point.Y));
        return new Point(x, y);
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint lpPoint);

    private static bool TryGetCursorScreenPosition(out Point cursor)
    {
        if (GetCursorPos(out var nativePoint))
        {
            cursor = new Point(nativePoint.X, nativePoint.Y);
            return true;
        }

        cursor = default;
        return false;
    }

    private void ShowSpeech(string text)
    {
        _speechWindow.SetText(text);
        if (!_speechWindow.IsVisible)
        {
            _speechWindow.Show();
        }

        Dispatcher.BeginInvoke(UpdateSpeechWindowPosition, DispatcherPriority.Loaded);

        _speechTimer.Stop();
        _speechTimer.Start();
    }

    private void AdvanceIdleLoopFrame()
    {
        switch (_animationMode)
        {
            case PetAnimationMode.WalkLoop:
                AdvanceWalkLoopFrame();
                return;
            case PetAnimationMode.IdleLoop:
                AdvanceIdleAnimationFrame();
                return;
            default:
                ApplyCurrentAnimationFrame();
                return;
        }
    }

    private void AdvanceIdleAnimationFrame()
    {
        if (_idleLoopFrames.Count == 0)
        {
            return;
        }

        if (_idleLoopFrames.Count == 1)
        {
            ApplyPetFrame(_idleLoopFrames[0]);
            return;
        }

        var nextIndex = _idleLoopFrameIndex + _idleLoopDirection;
        if (nextIndex >= _idleLoopFrames.Count)
        {
            _idleLoopDirection = -1;
            nextIndex = _idleLoopFrames.Count - 2;
        }
        else if (nextIndex < 0)
        {
            _idleLoopDirection = 1;
            nextIndex = 1;
        }

        _idleLoopFrameIndex = nextIndex;
        ApplyPetFrame(_idleLoopFrames[_idleLoopFrameIndex]);
    }

    private void AdvanceWalkLoopFrame()
    {
        if (_walkLoopFrames.Count == 0)
        {
            SetAnimationMode(GetIdleAnimationMode());
            return;
        }

        _walkLoopFrameIndex = (_walkLoopFrameIndex + 1) % _walkLoopFrames.Count;
        ApplyPetFrame(_walkLoopFrames[_walkLoopFrameIndex]);
    }

    private void ApplyCurrentAnimationFrame()
    {
        switch (_animationMode)
        {
            case PetAnimationMode.WalkLoop when _walkLoopFrames.Count > 0:
                ApplyPetFrame(_walkLoopFrames[_walkLoopFrameIndex]);
                return;
            case PetAnimationMode.IdleLoop when _idleLoopFrames.Count > 0:
                ApplyPetFrame(_idleLoopFrames[_idleLoopFrameIndex]);
                return;
            default:
                if (_idleLoopFrames.Count > 0)
                {
                    ApplyPetFrame(_idleLoopFrames[0]);
                }

                return;
        }
    }

    private void ApplyPetFrame(BitmapSource frame)
    {
        PetImage.Source = frame;
    }

    private void ScheduleNextBlink()
    {
        _blinkIntervalTimer.Stop();
        _blinkIntervalTimer.Interval = TimeSpan.FromMilliseconds(_random.Next(2200, 4200));
        _blinkIntervalTimer.Start();
    }

    private void StartBlinkSequence()
    {
        _blinkIntervalTimer.Stop();
        if (_blinkFrames.Count == 0)
        {
            return;
        }

        _blinkFrameIndex = 0;
        ApplyBlinkFrame();
        _blinkFrameTimer.Start();
    }

    private void AdvanceBlinkFrame()
    {
        _blinkFrameIndex++;
        if (_blinkFrameIndex >= _blinkFrames.Count)
        {
            _blinkFrameTimer.Stop();
            BlinkOverlayImage.Source = null;
            BlinkOverlayImage.Visibility = Visibility.Collapsed;
            _blinkFrameIndex = -1;
            ScheduleNextBlink();
            return;
        }

        ApplyBlinkFrame();
    }

    private void ApplyBlinkFrame()
    {
        var frame = _blinkFrames[_blinkFrameIndex];
        if (frame is null)
        {
            BlinkOverlayImage.Source = null;
            BlinkOverlayImage.Visibility = Visibility.Collapsed;
            return;
        }

        BlinkOverlayImage.Source = frame;
        BlinkOverlayImage.Visibility = Visibility.Visible;
    }

    private void UpdateSpeechWindowPosition()
    {
        if (!_speechWindow.IsLoaded && !_speechWindow.IsVisible)
        {
            return;
        }

        _speechWindow.UpdateLayout();
        var bubbleLeft = Left - _speechWindow.ActualWidth + 26;
        var bubbleTop = Top + Math.Max(18, (_petDisplayHeight * 0.18));
        _speechWindow.Left = bubbleLeft;
        _speechWindow.Top = bubbleTop;
    }

    private void SpeakRandomLine()
    {
        ShowSpeech(_greetingLines[_random.Next(_greetingLines.Length)]);
    }

    private void PetDragSurface_OnMouseEnter(object sender, MouseEventArgs e)
    {
    }

    private void PetDragSurface_OnMouseLeave(object sender, MouseEventArgs e)
    {
    }

    private void OnShellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _pointerDown = true;
        _dragging = false;
        _dragStartScreen = PointToScreen(e.GetPosition(this));
        _dragStartWindow = new Point(Left, Top);
        if (sender is IInputElement inputElement)
        {
            Mouse.Capture(inputElement);
        }
        e.Handled = true;
    }

    private void OnShellMouseMove(object sender, MouseEventArgs e)
    {
        if (!_pointerDown || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var screenPoint = PointToScreen(e.GetPosition(this));
        var dx = screenPoint.X - _dragStartScreen.X;
        var dy = screenPoint.Y - _dragStartScreen.Y;

        if (!_dragging && Math.Abs(dx) + Math.Abs(dy) > 4)
        {
            _dragging = true;
        }

        if (!_dragging)
        {
            return;
        }

        Left = _dragStartWindow.X + dx;
        Top = _dragStartWindow.Y + dy;
        UpdateSpeechWindowPosition();
    }

    private void OnShellMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_pointerDown)
        {
            return;
        }

        Mouse.Capture(null);
        var wasDragging = _dragging;
        _pointerDown = false;
        _dragging = false;

        if (wasDragging)
        {
            SetMovementMode(PetMovementMode.Pinned);
            return;
        }

        SpeakRandomLine();
        e.Handled = true;
    }

    private void MenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = !MenuPopup.IsOpen;
    }

    private void GreetButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = false;
        ShowSpeech("既然你点开了，那我就认真陪你一会儿。");
    }

    private void WanderButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = false;
        SetMovementMode(PetMovementMode.Wander);
        ShowSpeech("那我先自己在桌面上转一小圈。");
    }

    private void FollowMouseButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = false;
        SetMovementMode(PetMovementMode.FollowMouse);
        ShowSpeech("别乱晃哦，我会慢慢跟着你。");
    }

    private void PinPositionButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = false;
        SetMovementMode(PetMovementMode.Pinned);
        ShowSpeech("好吧，那我就先乖乖待在这里。");
    }

    private void MenuPopup_OnClosed(object? sender, EventArgs e)
    {
    }

    private void ExitPetButton_OnClick(object sender, RoutedEventArgs e)
    {
        MenuPopup.IsOpen = false;
        Close();
    }
}
