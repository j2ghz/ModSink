module DynamicDataEx

open DynamicData

let transformMany (valueTransform : 'a -> 'b seq) (keyTransform : 'b -> 'c) 
    source =
    ObservableCacheEx.TransformMany(source, valueTransform, keyTransform)
let filter filter reapply source =
    ObservableCacheEx.Filter(source, filter, reapply)
let autoRefresh prop source =
    ObservableCacheEx.AutoRefresh(source, propertyAccessor = prop)
let ``return`` value = System.Reactive.Linq.Observable.Return(value)
