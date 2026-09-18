type BrandLogoProps = {
  compact?: boolean;
};

export function BrandLogo({ compact = false }: BrandLogoProps) {
  return (
    <span className={compact ? "brand-lockup is-compact" : "brand-lockup"}>
      <img src="/yjcabin-puppy.webp" alt="" />
      <span className="brand-word">YJCabin</span>
    </span>
  );
}
