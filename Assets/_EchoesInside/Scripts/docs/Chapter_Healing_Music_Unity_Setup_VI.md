# Thiết lập nhạc trước và sau chữa lành trong Unity

## 1. Quy tắc chung

Trong mỗi scene chỉ giữ một object phát BGM. Nếu scene đã có `BGM`, `SceneMusic` hoặc object tương tự thì dùng lại object đó, không tạo object thứ hai.

Trên object BGM cần có:

- `Audio Source`
- `Scene Music Player`
- `Chapter Music State Player` chỉ dành cho các scene phòng bà, phòng ba và phòng mẹ

Thiết lập `Audio Source`:

```text
Audio Resource: None
Play On Awake: Off
Loop: On
Spatial Blend: 0 (2D)
```

Thiết lập `Scene Music Player` cho các phòng có hội thoại:

```text
Music Clip: None
Target Volume: 0.25
Loop: On
Play On Start: On
Fade In Duration: 1.2
```

`Music Clip` được để trống trong các phòng có `Chapter Music State Player`, vì component này sẽ tự chọn nhạc dựa trên trạng thái trong `GameManager`.

## 2. Chapter 1 - Bà

Mở scene `Chapter1_Grandma`.

Trên object BGM, thêm `Chapter Music State Player` và gán:

```text
Chapter: Grandma
Music Player: SceneMusicPlayer trên cùng object
Before Healing Clip: Noisy Memory [Soundtrack] 3N.wav
Healed Clip: To You [Old Radio Piano] (Ambience) 3N.wav
Transition Duration: 1.2
```

Chọn object có component `Puzzle Complete Handler`, sau đó kéo component `Chapter Music State Player` của object BGM vào ô `Chapter Music`.

Nhạc cũ tiếp tục phát trong lúc ghép và lấy ảnh. Khi nhấn quay lại phòng sáng, nhạc cũ fade-out 1.2 giây rồi nhạc ấm fade-in 1.2 giây.

## 3. Chapter 2 - Ba

Mở scene `Chapter2_Father`.

Trên object BGM, thêm `Chapter Music State Player` và gán:

```text
Chapter: Father
Music Player: SceneMusicPlayer trên cùng object
Before Healing Clip: Week 19 - Dark Portents SHROUDED FUTURE.ogg
Healed Clip: Week 20 - Dust Bowl HOPE OF RAIN.ogg
Transition Duration: 1.2
```

Khi `Maze Completed` còn tắt, phòng phát nhạc trước chữa lành. Sau khi hoàn thành mê cung, mọi lần quay lại phòng ba đều dùng nhạc ấm.

Mở scene `Chapter2_Maze`. Scene này không cần `Chapter Music State Player`. Gán trực tiếp:

```text
Scene Music Player > Music Clip: sp-underthestairs.wav
Target Volume: 0.28
Loop: On
Play On Start: On
Fade In Duration: 1.2
```

## 4. Chapter 3 - Mẹ

Mở scene `Chapter3_MomRoom`.

Trên object BGM, thêm `Chapter Music State Player` và gán:

```text
Chapter: Mother
Music Player: SceneMusicPlayer trên cùng object
Before Healing Clip: Sketchbook 2025-12-11_PIANO BREAK.ogg
Healed Clip: Sketchbook 2025-12-17_CHILL.ogg
Transition Duration: 1.2
```

Khi `Meal Completed` còn tắt, phòng phát nhạc trước chữa lành. Sau khi hoàn thành màn bếp, mọi lần quay lại phòng mẹ đều dùng nhạc ấm.

Mở scene `Chapter3_Kitchen`. Scene này không cần `Chapter Music State Player`. Gán trực tiếp:

```text
Scene Music Player > Music Clip: Week 23 - Workshop BREADBOARD.ogg
Target Volume: 0.28
Loop: On
Play On Start: On
Fade In Duration: 1.2
```

## 5. Kiểm tra

1. Lưu từng scene bằng `Ctrl + S`.
2. Kiểm tra mỗi scene chỉ có một `Scene Music Player` và một BGM `Audio Source`.
3. Chơi Chapter 1 và xác nhận nhạc chỉ đổi khi quay lại phòng sáng.
4. Chơi Chapter 2 và xác nhận phòng ba trước, mê cung và phòng ba sau dùng ba bài khác nhau.
5. Chơi Chapter 3 và xác nhận phòng mẹ trước, màn bếp và phòng mẹ sau dùng ba bài khác nhau.
6. Rời phòng rồi quay lại sau chữa lành; nhạc ấm phải được phát ngay từ đầu.
7. Nếu nhạc át lời thoại, giảm `Target Volume` xuống `0.22`.

## 6. Lỗi thường gặp

- `SceneMusicPlayer has no music clip assigned`: chưa gán clip gameplay hoặc chưa thêm `Chapter Music State Player` cho phòng.
- `ChapterMusicStatePlayer has no clip for the current state`: thiếu một trong hai clip Before/Healed.
- Chapter 1 không đổi nhạc: chưa gán ô `Puzzle Complete Handler > Chapter Music`.
- Hai bài phát cùng lúc: scene đang có nhiều hơn một BGM `Audio Source`; tắt hoặc xóa object BGM cũ không còn dùng.
