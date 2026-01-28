# Báo cáo Lab 4 - Audio Optimization

## Mục tiêu

Hiểu cách cấu hình AudioClip Import Settings để tối ưu hiệu suất và chất lượng âm thanh.

---

## Các thông số quan trọng

### 1. Load Type

| Load Type | Mô tả | Ưu điểm | Nhược điểm | Use Case |
|-----------|-------|---------|------------|----------|
| **Decompress On Load** | Giải nén toàn bộ vào RAM khi load | Play ngay, không CPU overhead | Tốn nhiều RAM | SFX ngắn (< 1s) |
| **Compressed In Memory** | Lưu nén trong RAM, giải nén khi play | Tiết kiệm RAM hơn | CPU overhead khi play | Voice, dialogue |
| **Streaming** | Stream trực tiếp từ disk | Tiết kiệm RAM nhất | Có latency, tốn disk I/O | BGM, ambient (dài) |

### 2. Compression Format

| Format | Quality | Size | CPU Usage | Use Case |
|--------|---------|------|-----------|----------|
| **PCM** | Tốt nhất | Lớn nhất | Thấp | Audio quan trọng, short clips |
| **ADPCM** | Tốt | Trung bình (3.5x nén) | Thấp | SFX, footsteps |
| **Vorbis** | Tùy chỉnh | Nhỏ nhất (10x+ nén) | Cao hơn | BGM, voice |

### 3. Sample Rate Setting

| Setting | Mô tả |
|---------|-------|
| **Preserve Sample Rate** | Giữ nguyên sample rate gốc |
| **Optimize Sample Rate** | Unity tự động chọn |
| **Override Sample Rate** | Tự chỉ định (8000-96000 Hz) |

---

## Recommended Settings theo loại Audio

### Background Music (BGM)

```
Load Type: Streaming
Compression: Vorbis (Quality 70-100%)
Sample Rate: Optimize
Preload Audio Data: OFF
```

**Lý do**: BGM thường dài (2-5 phút), streaming tiết kiệm RAM, latency không quan trọng vì chạy liên tục.

### Sound Effects (SFX)

```
Load Type: Decompress On Load
Compression: ADPCM hoặc PCM
Sample Rate: Preserve
Preload Audio Data: ON
```

**Lý do**: SFX cần play ngay lập tức (no latency), thường ngắn nên không tốn nhiều RAM.

### Voice/Dialogue

```
Load Type: Compressed In Memory
Compression: Vorbis (Quality 50-70%)
Sample Rate: Optimize (có thể giảm xuống 22050 Hz)
```

**Lý do**: Voice không cần chất lượng cao như nhạc, cân bằng giữa RAM và CPU.

### Ambient/Environment Loops

```
Load Type: Streaming
Compression: Vorbis (Quality 50-70%)
Sample Rate: Optimize
```

**Lý do**: Tương tự BGM, chạy liên tục, không cần instant play.

---

## So sánh Memory Usage

| Audio Type | Raw Size | PCM | ADPCM | Vorbis 70% |
|------------|----------|-----|-------|------------|
| BGM (3 min) | 31.5 MB | 31.5 MB | 9 MB | ~3 MB |
| SFX (0.5s) | 88 KB | 88 KB | 25 KB | ~9 KB |
| Voice (10s) | 1.7 MB | 1.7 MB | 500 KB | ~170 KB |

---

## Load Time Comparison

| Load Type | Initial Load | Play Latency |
|-----------|--------------|--------------|
| Decompress On Load | Slow (decode all) | Instant |
| Compressed In Memory | Fast | Small delay |
| Streaming | Fastest | Higher latency |

---

## Best Practices

1. **Luôn test trên target device** - Mobile có RAM/CPU giới hạn
2. **Sử dụng Audio Mixer** - Để quản lý volume và effects centrally
3. **Pool AudioSources** - Tránh instantiate/destroy liên tục
4. **Force To Mono** - Cho SFX 3D, stereo không cần thiết
5. **Reduce sample rate** - Voice/SFX có thể dùng 22050 Hz thay vì 44100 Hz

---

## Clips trong Lab này

### Clip 1: fah-469417.mp3

- **Type**: SFX / Jingle
- **Recommended**: Decompress On Load + ADPCM
- **Use Case**: UI feedback, notification

### Clip 2: laser-pistol-shooting-95497.mp3

- **Type**: SFX
- **Recommended**: Decompress On Load + ADPCM
- **Use Case**: Weapon sounds, instant play critical

### Clip 3: podcast-stinger-calm-professional-transition-469111.mp3

- **Type**: BGM / Transition
- **Recommended**: Streaming + Vorbis
- **Use Case**: Background music, transitions

---

## Kết luận

Việc chọn cấu hình phù hợp cho AudioClip phụ thuộc vào:

1. **Độ dài audio** - Ngắn → Decompress, Dài → Streaming
2. **Tần suất sử dụng** - Thường xuyên → Preload, Hiếm → Load khi cần
3. **Yêu cầu latency** - Instant → Decompress + PCM/ADPCM
4. **Platform constraints** - Mobile cần optimize RAM nhiều hơn

Không có cấu hình "one-size-fits-all", cần test và điều chỉnh theo từng project.
