import { Input as BaseInput } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Input.module.css";

export function Input({
    className,
    ...props
}: ComponentProps<typeof BaseInput>) {
    return (
        <BaseInput {...props} className={[Styles.input, className].join(" ")} />
    );
}
