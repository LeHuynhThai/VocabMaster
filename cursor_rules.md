# Cursor Rule: Không upload thư mục .cursor lên GitHub

- Luôn thêm `.cursor/` vào file `.gitignore`.
- Không commit hoặc push thư mục `.cursor` lên bất kỳ hệ thống quản lý mã nguồn nào (GitHub, GitLab, Bitbucket, ...).
- Lý do: Thư mục này chứa dữ liệu tạm thời, cache, hoặc thông tin cá nhân liên quan đến AI/Cursor, không liên quan đến source code chính thức của dự án. 