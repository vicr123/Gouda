import { Separator as BaseSeparator } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Separator.module.css";

export function Separator({
    className,
    ...props
}: ComponentProps<typeof BaseSeparator>) {
    return (
        <BaseSeparator
            {...props}
            className={[Styles.separator, className].join(" ")}
        />
    );
}
