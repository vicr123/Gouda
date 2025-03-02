import { ComponentProps } from "react";
import Styles from "./Textarea.module.css";

export function Textarea({ className, ...props }: ComponentProps<"textarea">) {
    return (
        <textarea
            {...props}
            className={[Styles.textarea, className].join(" ")}
        />
    );
}
