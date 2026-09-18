import path from "node:path";
import { fileURLToPath } from "node:url";
import sharp from "sharp";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const pub = path.join(root, "public");

const images = [
  { file: "yjcabin-puppy.png", width: 256 },
  { file: "yjcabin-cat.png", width: 192 },
  { file: "yjcabin-bird.png", width: 160 },
  { file: "yjcabin-paw.png", width: 128 }
];

for (const image of images) {
  const input = path.join(pub, image.file);
  const webp = path.join(pub, image.file.replace(/\.png$/i, ".webp"));
  await sharp(input)
    .resize({ width: image.width, withoutEnlargement: true })
    .webp({ quality: 74, effort: 6 })
    .toFile(webp);
  console.log("wrote", path.basename(webp));
}

await sharp(path.join(pub, "yjcabin-puppy.png"))
  .resize(32, 32, { fit: "cover" })
  .png({ compressionLevel: 9 })
  .toFile(path.join(pub, "favicon.png"));
console.log("wrote favicon.png");
