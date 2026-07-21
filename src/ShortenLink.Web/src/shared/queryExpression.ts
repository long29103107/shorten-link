export type FilterValue = string | number | boolean | Date;
export type FilterOperator = "eq" | "ne" | "gt" | "ge" | "lt" | "le" | "contains" | "startsWith" | "in";
export type FilterExpression =
  | { kind: "condition"; field: string; operator: FilterOperator; value: FilterValue | readonly FilterValue[] }
  | { kind: "group"; operator: "and" | "or"; expressions: readonly FilterExpression[] }
  | { kind: "not"; expression: FilterExpression };

export type SortExpression<TField extends string = string> = {
  field: TField;
  direction?: "asc" | "desc";
};

export const filter = {
  condition: (field: string, operator: FilterOperator, value: FilterValue | readonly FilterValue[]): FilterExpression => ({ kind: "condition", field, operator, value }),
  and: (...expressions: FilterExpression[]): FilterExpression => ({ kind: "group", operator: "and", expressions }),
  or: (...expressions: FilterExpression[]): FilterExpression => ({ kind: "group", operator: "or", expressions }),
  not: (expression: FilterExpression): FilterExpression => ({ kind: "not", expression })
};

export function serializeFilter(expression: FilterExpression): string {
  if (expression.kind === "not") return `!${serializeFilter(expression.expression)}`;
  if (expression.kind === "group") {
    if (expression.expressions.length === 0) throw new Error("A filter group needs at least one expression.");
    const separator = expression.operator === "and" ? " & " : " | ";
    return `(${expression.expressions.map(serializeFilter).join(separator)})`;
  }
  assertIdentifier(expression.field);
  const rawValue = Array.isArray(expression.value)
    ? `[${expression.value.map(serializeValue).join(",")}]`
    : serializeValue(expression.value as FilterValue);
  return `(${expression.field} ${expression.operator} \`${rawValue}\`)`;
}

export function serializeSort<TField extends string>(expressions: readonly SortExpression<TField>[]): string {
  return expressions.map(({ field, direction = "asc" }) => {
    assertIdentifier(field);
    return `${direction === "desc" ? "-" : "+"}${field}`;
  }).join(",");
}

export function appendQueryExpression(
  params: URLSearchParams,
  options: { filter?: FilterExpression; sort?: readonly SortExpression[] }
): URLSearchParams {
  if (options.filter) params.set("fe", serializeFilter(options.filter));
  if (options.sort?.length) params.set("sort", serializeSort(options.sort));
  return params;
}

function serializeValue(value: FilterValue): string {
  const serialized = value instanceof Date ? value.toISOString() : String(value);
  if (serialized.includes("`")) throw new Error("Filter values cannot contain backticks.");
  return serialized;
}

function assertIdentifier(value: string): void {
  if (!/^[A-Za-z_][A-Za-z0-9_.]*$/.test(value)) throw new Error(`Invalid query field: ${value}`);
}
