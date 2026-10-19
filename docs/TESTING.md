# Kiểm thử – HW1019 Trò chơi cờ caro

Tài liệu ghi lại cách kiểm thử và kết quả thực tế của bài làm.

## Môi trường

| Thành phần | Phiên bản |
| --- | --- |
| Hệ điều hành | Windows 11 Home 10.0.26200, display scaling 125% |
| Visual Studio / MSBuild | Visual Studio Community 2026 (MSBuild 18) |
| .NET SDK / runtime | SDK 10.0.401, runtime 8.0.31 |
| Windows App Runtime | 2.5.1 (x64) |

## 1. Build

| Lệnh | Kết quả |
| --- | --- |
| `MSBuild caro-game.slnx /restore /t:Build /p:Configuration=Debug /p:Platform=x64` | Thành công, 0 lỗi, 0 cảnh báo; tạo gói `caro-game (Package)_1.0.0.0_x64_Debug.msix` |
| `MSBuild caro-game.slnx /t:Rebuild /p:Configuration=Release /p:Platform=x64` | Thành công, 0 lỗi, 0 cảnh báo (bản Release bật trimming và ReadyToRun theo template) |

## 2. Unit test (xUnit)

```powershell
dotnet test .\caro-game.Tests\caro-game.Tests.csproj
```

Kết quả: **Passed 60, Failed 0, Skipped 0**.

| Lớp test | Số test | Nội dung chính |
| --- | ---: | --- |
| `GameRulesTests` | 29 | Nhận n = 4…20, từ chối n < 4 hoặc n > 20; số quân để thắng (4 cho bàn 4 × 4, 5 cho bàn lớn hơn); phân loại dữ liệu nhập: trống, không phải số nguyên (`abc`, `1.5`, `1e1`, `1 0`), ngoài khoảng (`3`, `21`, `-10`, số quá lớn) |
| `BoardTests` | 8 | Đặt quân, không đặt đè, vị trí ngoài bàn, bàn đầy, xóa quân, làm trống bàn |
| `WinCheckerTests` | 11 | Thắng ngang/dọc/chéo chính/chéo phụ; 4 quân chưa thắng; 6 quân vẫn thắng; bàn 4 × 4; chặn hai đầu (bật/tắt); chặn một đầu vẫn thắng; mép bàn không tính là chặn |
| `GameEngineTests` | 12 | Người đi trước, luân phiên lượt, đánh lại ô cũ, đánh ra ngoài, thắng, lấp chỗ trống giữa hàng để thắng, khóa bàn sau khi kết thúc, hòa khi kín bàn 4 × 4, luật chặn hai đầu, hoàn tác, chơi lại |

## 3. Kiểm thử giao diện

Ứng dụng được đăng ký từ thư mục layout của project đóng gói (giống thao tác F5 của Visual Studio) và chạy trong context gói MSIX. Các thao tác được thực hiện bằng **UI Automation** (nhập `TextBox`, bấm nút theo `AutomationId`, bấm ô cờ theo tên *"Hàng r, cột c: …"*) và đọc lại trạng thái từ cây UI Automation. Ảnh được chụp từ cửa sổ ứng dụng.

| # | Kịch bản | Kết quả mong đợi | Kết quả |
| ---: | --- | --- | --- |
| 1 | Mở ứng dụng | Màn hình thiết lập, n = 15 mặc định, dòng tóm tắt *Bàn cờ 15 × 15 (225 ô)* | Đạt – [01-setup.png](screenshots/01-setup.png) |
| 2 | Nhập `25` | Báo *Kích thước phải nằm trong khoảng 4 đến 20.*, nút bắt đầu bị vô hiệu | Đạt – [02-validation.png](screenshots/02-validation.png) |
| 3 | Nhập `abc`, rồi để trống | Báo lỗi tương ứng, nút bắt đầu bị vô hiệu | Đạt (`IsEnabled = False` ở cả hai trường hợp) |
| 4 | Tên X = `An`, tên O = `an` | Báo *Tên hai người chơi phải khác nhau.*, nút bắt đầu bị vô hiệu | Đạt |
| 5 | n = 15, bắt đầu, đánh 5 nước | Bàn cờ có 225 ô; X/O luân phiên; ô vừa đánh được tô; trạng thái *Lượt của Người chơi O · Nước đi thứ 6* | Đạt – [03-playing.png](screenshots/03-playing.png) |
| 6 | X xếp 5 quân (thử cả đường chéo và hàng ngang) | Báo *… thắng!*, tô vàng 5 quân, InfoBar *Ván đấu kết thúc*, tỉ số X = 1; nút Hoàn tác và các ô trống bị khóa | Đạt – [04-win.png](screenshots/04-win.png) |
| 7 | Bấm *Ván mới* sau khi thắng | Bàn cờ trống, *Ván 2*, O đi trước, tỉ số giữ nguyên | Đạt |
| 8 | Đánh 3 nước rồi bấm *Hoàn tác* | Ô cuối trở lại trống, lượt trả về người vừa đánh, ô đánh trước đó được tô | Đạt |
| 9 | Bấm *Ván mới* khi ván đang chơi | Hiện `ContentDialog` xác nhận; chọn *Chơi tiếp* thì bàn cờ giữ nguyên | Đạt – [05-confirm-dialog.png](screenshots/05-confirm-dialog.png) |
| 10 | Bấm *Đổi kích thước* khi đã có tiến trình, xác nhận | Quay về màn hình thiết lập, các giá trị đã nhập vẫn còn | Đạt |
| 11 | n = 4, đánh kín 16 ô theo thế không có 4 quân liên tiếp | *Hòa cờ!*, tỉ số hòa = 1, InfoBar báo người đi trước ván sau | Đạt – [06-draw-4x4.png](screenshots/06-draw-4x4.png) |
| 12 | Thu cửa sổ về 900 × 720 và 720 × 640 epx | Bố cục hẹp: ba thẻ thành một hàng trên bàn cờ, không bị cắt, không cần cuộn ngang | Đạt – [07-narrow-layout.png](screenshots/07-narrow-layout.png) |
| 13 | n = 20, bật luật chặn hai đầu, tạo hàng `O X X X X X O` | Bàn có 400 ô; hàng 5 X bị chặn hai đầu không thắng; lượt chuyển sang O | Đạt – [08-blocked-rule-20x20.png](screenshots/08-blocked-rule-20x20.png) |
| 14 | Mở lại cửa sổ 1280 × 860 epx | Bố cục rộng: bàn cờ bên trái, bảng điều khiển 320 epx bên phải | Đạt |

Các kịch bản trên được viết thành script tự động [tests/ui/Run-UiRegression.ps1](../tests/ui/Run-UiRegression.ps1) (29 điểm kiểm tra). Kết quả trên bản **Debug x64 của mã nguồn cuối cùng: 29/29 PASS**. Các ảnh chụp được lấy trong những lượt chạy trước đó trên bản Release và Debug.

Chạy lại (cần chạy ứng dụng bằng F5 trong Visual Studio một lần để gói được đăng ký; không dùng chuột/bàn phím trên cửa sổ game trong lúc script chạy):

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\ui\Run-UiRegression.ps1
```

Ghi chú: ảnh hộp thoại xác nhận được chụp trực tiếp từ màn hình, vì cách chụp `PrintWindow` dùng cho các ảnh khác không vẽ lại nội dung bàn cờ nằm dưới lớp phủ của `ContentDialog`.

## 4. Vấn đề phát hiện khi kiểm thử và cách xử lý

| Vấn đề | Nguyên nhân | Xử lý |
| --- | --- | --- |
| Build gói báo `NETSDK1047` cho `caro-game.Core` | Project đóng gói publish ứng dụng theo runtime `win-x64` và truyền runtime đó xuống thư viện | Khai báo `RuntimeIdentifiers` trong `caro-game.Core.csproj` |
| Script UI thỉnh thoảng đọc trạng thái trước khi giao diện kịp cập nhật | Kiểm tra ngay sau thao tác, không chờ | Thêm `Wait-Until` (chờ có giới hạn thời gian) cho các bước chuyển trang và mở hộp thoại |
| Bố cục hẹp ban đầu phải cuộn ngang mới thấy thẻ thao tác | Ba thẻ rộng cố định 320 epx xếp ngang | Đổi sang lưới 3 cột co giãn, ẩn gợi ý phím tắt khi hẹp, rút gọn nhãn nút |
| Bản Release lỗi `TypeLoadException` khi vừa build Debug xong | Template dùng chung thư mục `bin\win-x64\publish\` cho Debug và Release, file runtime đã trim bị trộn | Clean thư mục publish rồi build lại; README ghi chú *Clean Solution* khi đổi cấu hình |

## 5. Phần chưa kiểm thử tự động

- Chơi bằng bàn phím (phím mũi tên, Enter/Space) và tooltip khi rê chuột.
- Giao diện ở theme tối và tương phản cao (đã định nghĩa màu riêng cho từng theme nhưng chưa chụp ảnh).
- Nền tảng x86 và ARM64 (chỉ build và chạy x64).
