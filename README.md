# LABC4 - Unity Audio & Video Labs

## Mục tiêu

Giúp sinh viên nắm vững hệ thống Audio và Video trong Unity, bao gồm AudioSource, AudioListener, AudioClip và VideoPlayer.

## Quick Start

1. Mở Unity Project
2. Menu: **Tools → LABC4 → Setup Window**
3. Click **"🚀 Generate ALL Labs"**
4. Tất cả scenes sẽ được tạo trong `Assets/Scenes/`

## Cấu trúc Project

```
Assets/
├── Audio/
│   ├── BGM/              # Nhạc nền
│   └── SFX/              # Hiệu ứng âm thanh
├── Video/                # File video (.mp4)
├── Scenes/               # Các scene labs
│   ├── Lab1_AudioSourceBasic.unity
│   ├── Lab2_SpatialAudio.unity
│   ├── Lab3_GlobalAudioControl.unity
│   ├── Lab4_AudioOptimization.unity
│   ├── Lab5_VideoPlayerBasic.unity
│   ├── Lab6_VideoRenderTarget.unity
│   ├── Lab7_VideoEvents.unity
│   ├── MiniProject_IntroCutscene.unity
│   └── Gameplay.unity
├── Scripts/
│   ├── Audio/
│   │   ├── AudioTriggerController.cs
│   │   ├── SpatialAudioController.cs
│   │   ├── GlobalAudioController.cs
│   │   └── AudioClipManager.cs
│   ├── Video/
│   │   ├── VideoTriggerController.cs
│   │   ├── VideoRenderTargetController.cs
│   │   ├── VideoEventController.cs
│   │   └── IntroCutsceneManager.cs
│   ├── Player/
│   │   └── SimplePlayerController.cs
│   └── UI/
│       ├── LabInstructionUI.cs
│       ├── AudioStatusUI.cs
│       └── VideoEndUI.cs
├── Prefabs/
├── Materials/
├── RenderTextures/
└── Editor/
    └── LabSetupTools.cs
```

---

## Lab Details

### Lab 1 - AudioSource Basic (Sound Trigger)

**Mục tiêu**: Tạo AudioSource gắn với AudioClip, Play On Awake: OFF

| Phím | Chức năng |
|------|-----------|
| Space | Play Audio |
| S | Stop Audio |

### Lab 2 - Spatial Audio (2D vs 3D)

**Mục tiêu**: So sánh âm thanh 2D và 3D bằng Spatial Blend

| Phím | Chức năng |
|------|-----------|
| WASD | Di chuyển Player |
| Mouse | Xoay camera |
| T | Toggle 2D/3D mode |

**Khi nào dùng 2D/3D?**

- **2D (Spatial Blend = 0)**: UI sounds, background music, dialogue - âm thanh không phụ thuộc vị trí
- **3D (Spatial Blend = 1)**: Footsteps, environmental sounds, NPC voices - âm thanh phụ thuộc khoảng cách

### Lab 3 - Global Audio Control

**Mục tiêu**: Điều khiển global audio qua AudioListener

| Phím | Chức năng |
|------|-----------|
| M | Mute/Unmute (AudioListener.volume) |
| P | Pause/Resume (AudioListener.pause) |

### Lab 4 - AudioClip Import & Optimization

**Mục tiêu**: Hiểu các cấu hình AudioClip khác nhau

| Phím | Chức năng |
|------|-----------|
| 1-4 | Switch giữa các AudioClip |
| Space | Play/Pause |

**Xem báo cáo chi tiết**: [Audio Optimization Report](./AUDIO_OPTIMIZATION_REPORT.md)

### Lab 5 - VideoPlayer Basic

**Mục tiêu**: Import video và điều khiển cơ bản

| Phím | Chức năng |
|------|-----------|
| V | Play Video |
| Space | Pause/Resume |
| R | Restart |

### Lab 6 - Video Render Target

**Mục tiêu**: Hiển thị video qua RenderTexture

| Phím | Chức năng |
|------|-----------|
| Tab | Switch giữa UI RawImage và 3D Material |
| Space | Play/Pause |

### Lab 7 - Video Events

**Mục tiêu**: Sử dụng events prepareCompleted, loopPointReached

| Phím | Chức năng |
|------|-----------|
| V | Play Video |
| Space | Pause/Resume |
| R | Restart |
| C | Clear Event Log |

**Events được demo**:

- `prepareCompleted` - Video đã sẵn sàng phát
- `loopPointReached` - Video kết thúc (dù loop hay không)

### Mini Project - Intro Cutscene

**Mục tiêu**: Xây dựng màn hình intro hoàn chỉnh

**Features**:

- Video intro fullscreen + nhạc nền
- Nút Skip (ESC hoặc click)
- Fade in/out transitions
- Auto-load Gameplay scene khi video kết thúc

---

## Yêu cầu hệ thống

- Unity 2021.3 LTS hoặc mới hơn
- TextMeshPro package
- Video file: .mp4 format recommended
- Audio files: .mp3, .wav, .ogg

---

## Scripts Documentation

### AudioTriggerController

```csharp
// Public methods
void PlayAudio()      // Phát audio
void StopAudio()      // Dừng audio
void SetClip(clip)    // Đổi AudioClip
```

### SpatialAudioController

```csharp
void ToggleSpatialMode()    // Chuyển 2D ↔ 3D
void Set3DMode(bool)        // Set mode trực tiếp
```

### GlobalAudioController

```csharp
void ToggleMute()           // M key
void TogglePause()          // P key
void SetVolume(float)       // 0.0 - 1.0
```

### VideoEventController

```csharp
// Events
UnityEvent OnVideoPrepared
UnityEvent OnVideoStarted
UnityEvent OnVideoEnded

// End Actions
VideoEndAction.ShowUI       // Hiện UI panel
VideoEndAction.LoadScene    // Load scene khác
VideoEndAction.RestartVideo // Phát lại
```

### IntroCutsceneManager

```csharp
void SkipIntro()            // Skip và chuyển scene
```

---

## Troubleshooting

### Video không phát

1. Kiểm tra video format (.mp4 H.264 recommended)
2. Kiểm tra VideoPlayer.clip đã assigned
3. Gọi `Prepare()` trước khi `Play()`

### Audio không có âm thanh

1. Kiểm tra AudioListener trong scene (chỉ cần 1)
2. Kiểm tra AudioSource.volume > 0
3. Kiểm tra AudioListener.volume > 0

### 3D Audio không hoạt động

1. Kiểm tra Spatial Blend = 1
2. Kiểm tra Min/Max Distance settings
3. Di chuyển Player để test

---

## Author

Lab thực hành Chương 4 - Unity Audio & Video
