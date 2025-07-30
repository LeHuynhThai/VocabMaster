# VocabMaster

Ứng dụng học từ vựng Tiếng Anh với phát âm, nghĩa Tiếng Việt và ví dụ.

## Cài đặt

1. Clone repository:
   ```
   git clone https://github.com/yourusername/VocabMaster.git
   cd VocabMaster
   ```

2. Cài đặt các package cho backend:
   ```
   dotnet restore
   ```

3. Cài đặt các package cho frontend:
   ```
   cd VocabMaster.Client
   npm install
   ```

4. Cấu hình database:
   ```
   cd VocabMaster.API
   copy appsettings.example.json appsettings.json
   ```
   Sau đó chỉnh sửa chuỗi kết nối database trong appsettings.json (nếu cần)

5. Chạy migrations để tạo database:
   ```
   dotnet ef database update
   ```

## Cấu hình Google OAuth

1. Tạo project trên [Google Cloud Console](https://console.cloud.google.com/)
2. Thiết lập OAuth consent screen
3. Tạo OAuth client ID (Web application)
4. Thêm Authorized JavaScript origins: `http://localhost:3000`
5. Thêm Authorized redirect URIs: `http://localhost:3000`
6. Lấy Client ID và Client Secret, cập nhật vào file appsettings.json
   ```json
   "GoogleOAuth": {
     "ClientId": "YOUR_CLIENT_ID",
     "ClientSecret": "YOUR_CLIENT_SECRET",
     "RedirectUri": "http://localhost:3000"
   }
   ```
7. Cập nhật ClientID trong file `VocabMaster.Client/src/index.tsx`

## Chạy ứng dụng

1. Chạy backend:
   ```
   cd VocabMaster.API
   dotnet run
   ```

2. Chạy frontend (trong terminal khác):
   ```
   cd VocabMaster.Client
   npm start
   ```

3. Mở trình duyệt và truy cập: `http://localhost:3000`

## Tính năng chính

- Đăng ký, đăng nhập (JWT và Google OAuth)
- Phát sinh từ vựng ngẫu nhiên với phát âm, nghĩa Tiếng Việt
- Lưu từ vựng đã học và xem lại sau
- Tìm kiếm trong từ vựng đã học
- Quản lý hồ sơ người dùng (avatar, mật khẩu) 
