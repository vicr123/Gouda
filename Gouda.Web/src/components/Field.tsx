import { Field as BaseField } from "@base-ui-components/react";
import { ComponentProps } from "react";
import Styles from "./Field.module.css";
import { Input } from "./Input.tsx";

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
    Control: ({
        render,
        ...props
    }: ComponentProps<typeof BaseField.Control>) => (
        <BaseField.Control {...props} render={render ?? <Input />} />
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
