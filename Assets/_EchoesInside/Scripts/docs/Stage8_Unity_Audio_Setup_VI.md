# Giai đoạn 8 - Cài nhạc nền trong Unity

## 1. Kết quả phần code

Các script đã được chuẩn bị:

- `Narrative/SceneMusicPlayer.cs`: phát và fade-in nhạc của từng scene.
- `Narrative/SceneTransitionLoader.cs`: chuyển scene qua `ScreenFader` nếu có.
- `Narrative/ScreenFader.cs`: fade-out nhạc cùng lúc với fade tối màn hình.

Không cần gắn `SceneMusicPlayer` vào scene intro. Intro tiếp tục dùng hệ thống nhạc có sẵn trong `StaticCutscenePlayer`.

## 2. Chuẩn bị file nhạc

Trong cửa sổ Project:

1. Vào `Assets/_EchoesInside`.
2. Tạo folder `Audio`.
3. Trong `Audio`, tạo folder `Music`.
4. Kéo bảy file nhạc vào `Assets/_EchoesInside/Audio/Music`.
5. Đổi tên để dễ quản lý:

```text
BGM_Intro_Dinner
BGM_MainMap_DarkHouse
BGM_Grandma
BGM_FatherRoom
BGM_FatherMaze
BGM_MomRoom
BGM_MomKitchen
```

Chọn tất cả file nhạc, rồi đặt trong Inspector:

```text
Load Type: Compressed In Memory
Compression Format: Vorbis
Quality: 70
Preload Audio Data: bật
Load In Background: bật
Force To Mono: tắt
```

Nhấn `Apply`.

## 3. Gắn nhạc cho Intro

1. Mở scene `Intro_DinnerScene`.
2. Trong Hierarchy, chọn `IntroMusicSource`.
3. Ở component `Audio Source`, đặt:

```text
Play On Awake: tắt
Loop: bật
Volume: 0.30
Spatial Blend: 0 (2D)
```

4. Chọn `IntroCutsceneController`.
5. Trong `Static Cutscene Player`:

```text
Music Source: kéo IntroMusicSource vào
Background Music: kéo BGM_Intro_Dinner vào
```

6. Trong `Intro Start Controller`:

```text
Music Source: kéo IntroMusicSource vào
Music Fade Duration: 1.20
```

7. Nhấn `Ctrl+S`.

## 4. Tạo SceneMusic cho các scene còn lại

Thực hiện các bước dưới đây cho từng scene:

```text
SampleScene
Chapter1_Grandma
Chapter2_Father
Chapter2_Maze
Chapter3_MomRoom
Chapter3_Kitchen
```

### Tạo object

1. Mở scene.
2. Trong Hierarchy, nhấp chuột phải vào vùng trống.
3. Chọn `Create Empty`.
4. Đổi tên thành `SceneMusic`.
5. Chọn menu ba chấm của Transform, nhấn `Reset`.

### Thêm AudioSource

1. Chọn `SceneMusic`.
2. Nhấn `Add Component`.
3. Tìm và thêm `Audio Source`.
4. Đặt:

```text
Audio Resource/AudioClip: để trống
Play On Awake: tắt
Loop: bật
Volume: 1.00
Spatial Blend: 0 (2D)
Priority: 128
```

### Thêm SceneMusicPlayer

1. Vẫn chọn `SceneMusic`.
2. Nhấn `Add Component`.
3. Tìm `Scene Music Player`.
4. Đặt:

```text
Target Volume: 0.30
Loop: bật
Play On Start: bật
Fade In Duration: 1.20
```

5. Kéo đúng file vào ô `Music Clip`:

| Scene | Music Clip |
| --- | --- |
| `SampleScene` | `BGM_MainMap_DarkHouse` |
| `Chapter1_Grandma` | `BGM_Grandma` |
| `Chapter2_Father` | `BGM_FatherRoom` |
| `Chapter2_Maze` | `BGM_FatherMaze` |
| `Chapter3_MomRoom` | `BGM_MomRoom` |
| `Chapter3_Kitchen` | `BGM_MomKitchen` |

6. Nhấn `Ctrl+S` trước khi chuyển sang scene tiếp theo.

Lưu ý: trong giai đoạn này, mỗi chapter scene chỉ nên có một `AudioSource`. Code factory cũ đang tìm `AudioSource` đầu tiên trong scene.

## 5. Bổ sung FadeOverlay cho Chapter2_Maze

`Chapter2_Maze` hiện chưa có `ScreenFader`.

1. Mở `Chapter2_Father`.
2. Trong Hierarchy, chọn object `FadeOverlay`.
3. Nhấn `Ctrl+C`.
4. Mở `Chapter2_Maze`.
5. Nhấn `Ctrl+V` để dán `FadeOverlay` vào scene.
6. Chọn `FadeOverlay`, kiểm tra `Screen Fader`:

```text
Fade Duration: 1.20
Fade In On Start: bật
```

7. Kiểm tra Image màu đen phủ toàn màn hình và Canvas nằm trên UI mê cung.
8. Nhấn `Ctrl+S`.

## 6. Thứ tự kiểm tra

1. Mở `Intro_DinnerScene` và nhấn Play.
2. Kiểm tra chỉ có nhạc intro phát.
3. Chạy hết intro và sang `SampleScene`.
4. Kiểm tra nhạc intro nhỏ dần, sau đó nhạc main map lớn dần trong 1.2 giây.
5. Di chuyển qua bốn waypoint trong main map.
6. Nhạc main map phải tiếp tục từ vị trí hiện tại, không phát lại từ đầu.
7. Vào Chapter 1 và quay lại map.
8. Vào phòng Ba, sau đó vào mê cung và quay lại phòng Ba.
9. Vào phòng Mẹ, sau đó vào bếp và quay lại phòng Mẹ.
10. Mỗi lần load scene chỉ được nghe một bản nhạc nền.

## 7. Xử lý lỗi thường gặp

### Không nghe thấy nhạc

- Kiểm tra `Music Clip` trong `SceneMusicPlayer` đã được gắn.
- Kiểm tra scene có `AudioListener` trên Main Camera.
- Kiểm tra `Mute` của AudioSource đang tắt.
- Kiểm tra `Target Volume` lớn hơn `0`.

### Console báo chưa có music clip

Thông báo `SceneMusicPlayer has no music clip assigned` nghĩa là scene đó chưa được kéo file nhạc vào ô `Music Clip`.

### Hai bản nhạc phát cùng lúc

- Tìm `t:AudioSource` trong Hierarchy của scene.
- Giữ đúng một AudioSource dành cho BGM.
- Không bật `Play On Awake` trên AudioSource của `SceneMusic`.

### Nhạc đổi quá nhanh hoặc bị cắt

- Kiểm tra scene có `ScreenFader`.
- Đặt `Fade Duration` và `Fade In Duration` cùng bằng `1.20`.
- Riêng `Chapter2_Maze`, kiểm tra đã sao chép `FadeOverlay` vào scene.
