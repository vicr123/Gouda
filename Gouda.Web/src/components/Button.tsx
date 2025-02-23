import { ComponentProps, ReactNode } from "react";
import Styles from "./Button.module.css";

interface ButtonProps extends ComponentProps<"button"> {
    children: ReactNode;
    className?: string;
}

export function Button({ children, className, ...props }: ButtonProps) {
    return (
        <button className={[Styles.button, className].join(" ")} {...props}>
            {children}
        </button>
    );
}
