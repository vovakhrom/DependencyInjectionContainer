﻿using DependencyInjectionContainer.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public static class Creator
    {
        
        public static object GetInstance(Type type, DependenciesConfiguration dependencyConfiguration)
        {
            var constructors = ChooseConstructors(type).ToList();
            if (constructors.Count == 0) throw new CreatorException($"{type} has no injectable constructor");
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var arguments = ProvideParameters(parameters, dependencyConfiguration);
                return constructor.Invoke(arguments.ToArray());
            }

            throw new CreatorException($"Can't create instance of {type}");
        }

        private static IEnumerable<object> ProvideParameters(IEnumerable<ParameterInfo> parameters,
            DependenciesConfiguration dependencyConfiguration)
        {
            //создаем провайдер
            var provider = new DependencyProvider(dependencyConfiguration);
            //возвращаем объекты реализации интерфейса
            return parameters.Select(provider.Resolve);
        }

        private static IEnumerable<ConstructorInfo> ChooseConstructors(Type type)
        {
            return type.GetConstructors()
                .Where(HasConstructedParameters);
        }

        private static bool HasConstructedParameters(ConstructorInfo constructor)
        {
            return constructor.GetParameters()
                .All(IsParameterConstructable);
        }

        private static bool IsParameterConstructable(ParameterInfo parameter)
        {
            var parameterType = parameter.GetType();
            return parameterType.IsClass;
        }
    }
}
