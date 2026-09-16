# Hướng dẫn đổi ảnh intro tại câu thoại 11

## 1. Tính năng đã được bổ sung

`StaticCutscenePlayer` hiện đã hỗ trợ:

- Gán một ảnh nền tùy chọn cho từng câu thoại.
- Làm ảnh cũ tối dần thành màu đen, đổi ảnh, rồi làm ảnh mới sáng dần trở lại.
- Khóa phím chuyển thoại trong lúc ảnh đang fade để tránh bỏ qua câu tiếp theo.
- Giữ nguyên ảnh mới ở các câu sau nếu không gán thêm ảnh.
- Khôi phục ảnh ban đầu nếu phát lại cutscene trong cùng scene.

Bạn không cần tạo Animator, Timeline hoặc một `Background` thứ hai.

## 2. Đưa ảnh mới vào Unity

1. Mở Unity và chờ quá trình compile kết thúc.
2. Trong cửa sổ `Project`, mở thư mục:

   ```text
   Assets/_EchoesInside/Art/Backgrounds/House
   ```

3. Kéo file PNG chỉ còn người con trai đang ngồi từ File Explorer vào thư mục này.
4. Đổi tên dễ nhận biết, ví dụ:

   ```text
   Intro_PlayerAlone.png
   ```

5. Chọn ảnh vừa nhập và thiết lập trong `Inspector`:

   ```text
   Texture Type: Sprite (2D and UI)
   Sprite Mode: Single
   Pixels Per Unit: 100
   Filter Mode: Point (no filter)
   Compression: None
   Generate Mip Maps: Off
   ```

6. Nhấn `Apply`.

Nếu không kéo được ảnh vào ô `Background Sprite`, hãy kiểm tra lại `Texture Type`. Ảnh bắt buộc phải là `Sprite (2D and UI)`.

## 3. Mở scene intro

1. Trong cửa sổ `Project`, mở:

   ```text
   Assets/Scenes/Intro_DinnerScene.unity
   ```

2. Trong `Hierarchy`, chọn object:

   ```text
   IntroCutsceneController
   ```

3. Tìm component `Static Cutscene Player` trong `Inspector`.

## 4. Gán Background Image

1. Trong `Hierarchy`, mở object `Canvas`.
2. Tìm object con tên `Background`.
3. Kéo object `Background` từ `Hierarchy` vào ô:

   ```text
   Static Cutscene Player
   > Optional Background
   > Background Image
   ```

4. Đặt:

   ```text
   Background Fade Duration: 0.6
   ```

Không kéo file PNG vào ô `Background Image`. Ô này cần component UI `Image` từ object `Canvas/Background`.

## 5. Gán ảnh mới tại câu thoại 11

1. Trong component `Static Cutscene Player`, mở:

   ```text
   Cutscene
   > Lines
   ```

2. Nếu bạn đang dùng bản thoại rút gọn, đặt `Size = 14`.
3. Mở `Element 10`.

   Unity đếm từ số 0, vì vậy:

   ```text
   Element 0  = câu 1
   Element 9  = câu 10
   Element 10 = câu 11
   ```

4. Tại `Element 10`, tìm ô mới:

   ```text
   Background Sprite
   ```

5. Kéo `Intro_PlayerAlone.png` từ cửa sổ `Project` vào ô này.
6. Để trống `Background Sprite` ở:

   ```text
   Element 0 đến Element 9
   Element 11 đến Element 13
   ```

Các câu 12–14 sẽ tự giữ ảnh của câu 11. Không cần gán lại cùng một ảnh.

## 6. Lưu và kiểm tra

1. Nhấn `Ctrl+S` để lưu scene.
2. Nhấn Play từ `Intro_DinnerScene`.
3. Chuyển lần lượt qua mười câu đầu.

   Kết quả mong đợi: ảnh bữa cơm đầy đủ thành viên vẫn được giữ nguyên.

4. Chuyển sang câu 11.

   Kết quả mong đợi:

   - Nội dung câu 11 xuất hiện.
   - Ảnh cũ tối dần thành màu đen trong khoảng `0,3 giây`.
   - Khi màn hình đen hoàn toàn, ảnh đổi thành cảnh chỉ còn người con trai.
   - Ảnh mới sáng dần trở lại trong khoảng `0,3 giây`.

5. Trong lúc ảnh đang đổi, thử nhấn Space liên tục.

   Kết quả mong đợi: thoại không chuyển sang câu 12 cho đến khi fade hoàn tất.

6. Đi tiếp đến hết intro.

   Kết quả mong đợi: ảnh người con trai được giữ đến hết câu 14, sau đó transition hiện tại vẫn tải `SampleScene`.

## 7. Xử lý lỗi thường gặp

### Không thấy ô Background Image hoặc Background Sprite

- Mở `Console` và kiểm tra có lỗi compile màu đỏ hay không.
- Chờ Unity compile xong rồi chọn lại `IntroCutsceneController`.
- Không xóa component `Static Cutscene Player` cũ.

### Đến câu 11 nhưng ảnh không đổi

Kiểm tra lần lượt:

1. `Background Image` đã nhận đúng `Canvas/Background` chưa.
2. Ảnh mới đã được gán đúng vào `Element 10 > Background Sprite` chưa.
3. Ảnh mới có `Texture Type = Sprite (2D and UI)` chưa.
4. Bạn có đang dùng đúng scene `Intro_DinnerScene` không.

### Ảnh bị mờ hoặc không còn chất pixel

Đặt lại:

```text
Filter Mode: Point (no filter)
Compression: None
Generate Mip Maps: Off
```

Sau đó nhấn `Apply`.

### Ảnh bị méo hoặc không phủ đúng màn hình

Ảnh mới nên có cùng tỉ lệ và kích thước với ảnh intro cũ. Không thay đổi `RectTransform` của `Canvas/Background`; hệ thống chỉ đổi `Sprite`, nên hai ảnh cùng tỉ lệ sẽ giữ nguyên bố cục.

### Trong lúc đổi ảnh từng thấy nền màu xanh

Phiên bản code hiện tại không còn giảm alpha của ảnh xuống 0. Ảnh được giữ nguyên độ đục và chỉ tối màu dần về đen, vì vậy màu nền xanh của Camera sẽ không lộ ra. Bạn không cần đổi `Background Color` của Main Camera hoặc tạo thêm panel đen.

### Thoại không nhận phím trong khoảng 0,6 giây

Đây là hành vi đúng. Input được khóa tạm thời trong lúc đổi ảnh để người chơi không vô tình bỏ qua câu 11.
