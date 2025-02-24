import { Field as BaseField } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Field.module.css";

export const Field = {
    Root: ({ className, ...props }: ComponentProps<typeof BaseField.Root>) => (
        <BaseField.Root
            {...props}
            className={[Styles.root, className].join(" ")}
        />
    ),
    Label: ({ ...props }: ComponentProps<typeof BaseField.Label>) => (
        <BaseField.Label {...props} />
    ),
    Control: ({ ...props }: ComponentProps<typeof BaseField.Control>) => (
        <BaseField.Control {...props} />
    ),
    Error: ({ ...props }: ComponentProps<typeof BaseField.Error>) => (
        <BaseField.Error {...props} />
    ),
    Description: ({
        className,
        ...props
    }: ComponentProps<typeof BaseField.Description>) => (
        <BaseField.Description
            {...props}
            className={[Styles.description, className].join(" ")}
        />
    ),
};
