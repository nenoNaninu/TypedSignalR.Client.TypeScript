import { getHubProxyFactory } from '../generated/json-null/TypedSignalR.Client'
import { IUnaryHub } from '../generated/json-null/TypedSignalR.Client/TypedSignalR.Client.TypeScript.Tests.Shared';
import { MyRequestItem } from '../generated/json-null/TypedSignalR.Client.TypeScript.Tests.Shared';

type Expect<T extends true> = T;
type IsEqual<A, B> =
    (<T>() => T extends A ? 1 : 2) extends
    (<T>() => T extends B ? 1 : 2) ? true : false;

type IsOptional<T, K extends keyof T> = {} extends Pick<T, K> ? true : false;

type TextPropertyIsRequired = Expect<IsEqual<IsOptional<MyRequestItem, 'text'>, false>>;
type TextPropertyUsesNull = Expect<IsEqual<MyRequestItem['text'], string | null>>;
type NullableMethodParameter = Expect<IsEqual<Parameters<IUnaryHub['echoNullableText']>[0], string | null>>;
type NullableMethodReturn = Expect<IsEqual<Awaited<ReturnType<IUnaryHub['echoNullableText']>>, string | null>>;

test('nullableStrategy.test', () => {
    const item: MyRequestItem = { text: null };
    const factory = getHubProxyFactory("IUnaryHub");

    expect(item.text).toBeNull();
    expect(factory).toBeDefined();
});
