# YJCabin

个人作品与文章站点。Web（React + Vite）与 Windows（Avalonia）共用 `YJCabin.Server` 提供的 REST API。文章以 Markdown 为正文真源，作品元数据保存在 PostgreSQL。

## 结构

- `YJCabin.Server`：四层 Web API（Api / Application / Domain / Infrastructure）
- `YJCabin.Client/web`：公开站点 + 轻量后台
- `YJCabin.Client/desktop`：站长用 Avalonia CMS
- `content/articles`：文章 Markdown

## 本地开发

1. 启动 PostgreSQL（需要 Docker）：`docker compose up -d`
2. 启动 API：`dotnet run --project YJCabin.Server/YJCabin.Api --launch-profile http`
3. 启动 Web：在 `YJCabin.Client/web` 执行 `npm install` 后 `npm run dev`
4. 启动桌面 CMS：`dotnet run --project YJCabin.Client/desktop`

默认管理员：`admin` / `ChangeMe!123`  
Swagger：http://localhost:5178/swagger  
Web：http://localhost:5173  
后台：http://localhost:5173/admin/login
