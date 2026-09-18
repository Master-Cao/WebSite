type LoadingOverlayProps = {
  show: boolean;
  message?: string;
};

export function LoadingOverlay({ show, message = "正在加载…" }: LoadingOverlayProps) {
  if (!show) {
    return null;
  }

  return (
    <div className="loading-overlay" role="status" aria-live="polite" aria-busy="true">
      <img src="/yjcabin-paw.webp" alt="" className="loading-critter loading-critter-a" />
      <img src="/yjcabin-bird.webp" alt="" className="loading-critter loading-critter-b" />
      <img src="/yjcabin-cat.webp" alt="" className="loading-critter loading-critter-c" />
      <img src="/yjcabin-paw.webp" alt="" className="loading-critter loading-critter-d" />
      <div className="loading-card glass">
        <div className="loading-stage" aria-hidden="true">
          <img src="/yjcabin-paw.webp" alt="" className="loading-paw loading-paw-l" />
          <img src="/yjcabin-puppy.webp" alt="" className="loading-puppy" />
          <img src="/yjcabin-paw.webp" alt="" className="loading-paw loading-paw-r" />
        </div>
        <p>{message}</p>
      </div>
    </div>
  );
}
