export const ICP_NO = "渝ICP备2025050068号";
export const ICP_HREF = "https://beian.miit.gov.cn/";

export function BeianNotice({ className }: { className?: string }) {
  return (
    <div className={className ? `beian ${className}` : "beian"}>
      <a href={ICP_HREF} target="_blank" rel="noreferrer">
        {ICP_NO}
      </a>
      <p>本站内容仅供学习与交流。</p>
      <p>© 2026 YJCabin</p>
    </div>
  );
}
