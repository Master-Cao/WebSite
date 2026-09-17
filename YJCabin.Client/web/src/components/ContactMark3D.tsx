import { Canvas, useFrame } from "@react-three/fiber";
import { useLayoutEffect, useMemo, useRef } from "react";
import { Box3, ExtrudeGeometry, Group, Vector3 } from "three";
import { SVGLoader } from "three/examples/jsm/loaders/SVGLoader.js";
import type { ContactKind } from "../lib/contacts";

const iconSvg: Record<ContactKind, string> = {
  qq: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path fill="#000" d="M12 2.4c-2.2 0-4.4 1.7-4.8 4.3-.2 1.4.1 2.8.7 4-1.3.6-2.3 1.7-2.7 3.1-.2.8.4 1.3 1 1 .4-.1.7-.4.9-.7.2 1.3.9 2.5 1.9 3.3-.4.3-.9.8-.8 1.4.1.7.9 1.1 1.6 1.2 1.2.2 2.4 0 3.5-.4.4.2.9.3 1.4.3s1-.1 1.4-.3c1.1.4 2.3.6 3.5.4.7-.1 1.5-.5 1.6-1.2.1-.6-.4-1.1-.8-1.4 1-.8 1.7-2 1.9-3.3.2.3.5.6.9.7.6.2 1.2-.2 1-1-.4-1.4-1.4-2.5-2.7-3.1.6-1.2.9-2.6.7-4C16.4 4.1 14.2 2.4 12 2.4Z"/></svg>`,
  email: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256"><path fill="#000" d="M224,48H32a8,8,0,0,0-8,8V192a16,16,0,0,0,16,16H216a16,16,0,0,0,16-16V56A8,8,0,0,0,224,48Zm-8,144H40V74.19l82.59,75.71a8,8,0,0,0,10.82,0L216,74.19V192Z"/></svg>`,
  wechat: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256"><path fill="#000" d="M232.07,186.76A80,80,0,0,0,169.58,72.59,80,80,0,1,0,23.93,138.76l-7.27,24.71a16,16,0,0,0,19.87,19.87l24.71-7.27a79,79,0,0,0,25.19,7.35,80,80,0,0,0,108.33,40.65l24.71,7.27a16,16,0,0,0,19.87-19.87ZM132,152a12,12,0,1,1,12-12A12,12,0,0,1,132,152Zm-52,0a80.32,80.32,0,0,0,1.3,14.3,63.45,63.45,0,0,1-15.49-5.85,8,8,0,0,0-6-.63L32,168l8.17-27.76a8,8,0,0,0-.63-6A64,64,0,0,1,151.68,72.43,80.12,80.12,0,0,0,80,152Zm108,0a12,12,0,1,1,12-12A12,12,0,0,1,188,152Z"/></svg>`,
  github: `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256"><path fill="#000" d="M216,104v8a56.06,56.06,0,0,1-48.44,55.47A39.8,39.8,0,0,1,176,192v40a8,8,0,0,1-8,8H104a8,8,0,0,1-8-8V216H72a40,40,0,0,1-40-40A24,24,0,0,0,8,152a8,8,0,0,1,0-16,40,40,0,0,1,40,40,24,24,0,0,0,24,24H96v-8a39.8,39.8,0,0,1,8.44-24.53A56.06,56.06,0,0,1,56,112v-8a58.14,58.14,0,0,1,7.69-28.32A59.78,59.78,0,0,1,69.07,28,8,8,0,0,1,76,24a59.75,59.75,0,0,1,48,24h24a59.75,59.75,0,0,1,48-24,8,8,0,0,1,6.93,4,59.74,59.74,0,0,1,5.37,47.68A58,58,0,0,1,216,104Z"/></svg>`
};

function extrudeIcon(kind: ContactKind) {
  const parsed = new SVGLoader().parse(iconSvg[kind]);
  const shapes = parsed.paths.flatMap((path) => SVGLoader.createShapes(path));
  const depth = kind === "qq" ? 2.4 : 22;
  const geometry = new ExtrudeGeometry(shapes, {
    depth,
    bevelEnabled: true,
    bevelThickness: depth * 0.18,
    bevelSize: depth * 0.12,
    bevelSegments: 3,
    curveSegments: 12
  });
  geometry.computeBoundingBox();
  const box = geometry.boundingBox ?? new Box3();
  const size = box.getSize(new Vector3());
  const longest = Math.max(size.x, size.y, size.z, 1);
  geometry.center();
  geometry.scale(1.35 / longest, -1.35 / longest, 1.35 / longest);
  geometry.center();
  return geometry;
}

function IconMesh({ kind, spinning }: { kind: ContactKind; spinning: boolean }) {
  const group = useRef<Group>(null);
  const geometry = useMemo(() => extrudeIcon(kind), [kind]);

  useLayoutEffect(() => () => geometry.dispose(), [geometry]);

  useFrame((_, delta) => {
    if (group.current && spinning) {
      group.current.rotation.y += delta * 0.9;
    }
  });

  return (
    <group ref={group} rotation={[0.12, 0.35, 0]}>
      <mesh geometry={geometry}>
        <meshPhysicalMaterial
          color="#ffffff"
          metalness={0.12}
          roughness={0.22}
          clearcoat={0.7}
          clearcoatRoughness={0.16}
          emissive="#ffffff"
          emissiveIntensity={0.08}
        />
      </mesh>
    </group>
  );
}

export function ContactMark3D({ kind }: { kind: ContactKind }) {
  const reduceMotion = typeof window !== "undefined" && window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  return (
    <Canvas camera={{ position: [0, 0.1, 3.6], fov: 34 }} dpr={[1, 1.75]} gl={{ antialias: true, alpha: true }}>
      <ambientLight intensity={0.95} />
      <directionalLight position={[3.4, 3.2, 4.2]} intensity={1.55} color="#ffffff" />
      <directionalLight position={[-2.6, 0.4, 2.2]} intensity={0.45} color="#f4fbff" />
      <IconMesh kind={kind} spinning={!reduceMotion} />
    </Canvas>
  );
}
