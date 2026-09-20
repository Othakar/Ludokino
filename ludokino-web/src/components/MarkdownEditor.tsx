"use client";

import { useRef } from "react";
import type { ChangeEvent } from "react";
import ReactMarkdown from "react-markdown";
import { Bold, Code2, Heading1, Heading2, ImageIcon, Italic, Link as LinkIcon, List, Quote } from "lucide-react";

type MarkdownEditorProps = {
  value: string;
  onChange: (value: string) => void;
  label?: string;
};

export default function MarkdownEditor({ value, onChange, label = "Contenu" }: MarkdownEditorProps) {
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);

  const applySnippet = (prefix: string, suffix = "", placeholder = "texte") => {
    const textarea = textareaRef.current;
    if (!textarea) return;

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = textarea.value.slice(start, end) || placeholder;
    const nextValue = `${textarea.value.slice(0, start)}${prefix}${selected}${suffix}${textarea.value.slice(end)}`;
    onChange(nextValue);

    requestAnimationFrame(() => {
      textarea.focus();
      const caretStart = start + prefix.length;
      const caretEnd = caretStart + selected.length;
      textarea.setSelectionRange(caretStart, caretEnd);
    });
  };

  const handleChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
    onChange(event.target.value);
  };

  return (
    <div className="editor-shell">
      <label className="mono-font editor-label">{label}</label>
      <div className="editor-toolbar" aria-label="Outils d'édition Markdown">
        <button type="button" className="editor-button" onClick={() => applySnippet("# ", "", "Titre principal")} title="Titre principal"><Heading1 size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("## ", "", "Sous-titre")} title="Sous-titre"><Heading2 size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("**", "**", "gras")} title="Gras"><Bold size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("*", "*", "italique")} title="Italique"><Italic size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("- ", "", "élément de liste")} title="Liste"><List size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("> ", "", "Citation")} title="Citation"><Quote size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("[", "](https://example.com)", "libellé")} title="Lien"><LinkIcon size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("![", "](https://example.com/image.jpg)", "alt")} title="Image"><ImageIcon size={15} /></button>
        <button type="button" className="editor-button" onClick={() => applySnippet("```\n", "\n```", "code")} title="Bloc de code"><Code2 size={15} /></button>
      </div>
      <textarea
        ref={textareaRef}
        className="editor-input"
        value={value}
        onChange={handleChange}
        placeholder="# Titre

## Sous-titre

- liste
- autre point

[link](https://example.com)

![image](https://example.com/image.jpg)"
      />
      <div className="editor-preview">
        <div className="editor-preview-header mono-font">APERÇU</div>
        <ReactMarkdown
          components={{
            img: ({ src, alt }) => (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={typeof src === "string" ? src : ""} alt={alt ?? ""} />
            ),
            a: ({ href, children }) => (
              <a href={href} target="_blank" rel="noreferrer">
                {children}
              </a>
            ),
          }}
        >
          {value || "Votre aperçu s’affichera ici."}
        </ReactMarkdown>
      </div>
    </div>
  );
}
