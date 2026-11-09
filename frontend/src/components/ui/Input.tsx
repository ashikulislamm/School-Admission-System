import type { InputHTMLAttributes, Ref } from "react";
import { useId } from "react";
import { cn } from "@/lib/cn";

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "id"> {
  label: string;
  /** Visually hide the label (it stays available to screen readers). */
  hideLabel?: boolean;
  /** Error sentence. Sets the danger border; rendered below unless `hideErrorText`. */
  error?: string | null;
  /** The caller renders the error text itself (it must still be linked via aria-describedby). */
  hideErrorText?: boolean;
  /** Id of an element that already describes this input (used with `hideErrorText`). */
  describedBy?: string;
  ref?: Ref<HTMLInputElement>;
}

export function Input({
  label,
  hideLabel = false,
  error,
  hideErrorText = false,
  describedBy,
  className,
  ref,
  ...props
}: InputProps) {
  const id = useId();
  const errorId = `${id}-error`;
  const describedIds = [error && !hideErrorText ? errorId : null, describedBy].filter(Boolean).join(" ");

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={id} className={cn("text-label text-muted", hideLabel && "sr-only")}>
        {label}
      </label>
      <input
        id={id}
        ref={ref}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedIds || undefined}
        className={cn(
          "h-10 w-full rounded-control border bg-surface px-3 text-body text-fg placeholder:text-muted",
          error ? "border-danger" : "border-line-strong",
          className,
        )}
        {...props}
      />
      {error && !hideErrorText && (
        <p id={errorId} className="text-label font-normal text-danger">
          {error}
        </p>
      )}
    </div>
  );
}
