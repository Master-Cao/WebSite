import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import { X } from "@phosphor-icons/react";
import type { ContactCard } from "../lib/contacts";
import { ContactMark3D } from "./ContactMark3D";

type ContactCardDialogProps = {
  card: ContactCard;
  onClose: () => void;
};

export function ContactCardDialog({ card, onClose }: ContactCardDialogProps) {
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    const previous = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    window.addEventListener("keydown", onKey);
    return () => {
      document.body.style.overflow = previous;
      window.removeEventListener("keydown", onKey);
    };
  }, [onClose]);

  const copyValue = async () => {
    try {
      await navigator.clipboard.writeText(card.value);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  };

  return createPortal(
    <div className="contact-overlay" onClick={onClose} role="presentation">
      <div
        className="contact-dialog"
        role="dialog"
        aria-modal="true"
        aria-label={card.title}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="contact-dialog-stage" aria-hidden="true">
          <ContactMark3D kind={card.kind} />
        </div>
        <div className="contact-dialog-body">
          <button type="button" className="contact-dialog-close" onClick={onClose} aria-label="关闭">
            <X size={18} />
          </button>
          <p className="contact-dialog-value">{card.value}</p>
          <div className="contact-dialog-actions">
            <button type="button" className="btn btn-primary" onClick={copyValue}>
              {copied ? "已复制" : "复制"}
            </button>
            {card.href && (
              <a className="btn btn-ghost" href={card.href} target="_blank" rel="noreferrer">
                打开
              </a>
            )}
          </div>
        </div>
      </div>
    </div>,
    document.body
  );
}
